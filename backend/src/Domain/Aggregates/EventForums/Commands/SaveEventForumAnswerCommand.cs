using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums.Commands
{
    public class SaveEventForumAnswerCommand
    {
        public class Contract : CommandContract<Result<EventForumAnswer>>
        {
            public string QuestionId { get; set; }
            public string Text { get; set; }
            public User User { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<EventForumAnswer>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;
            private readonly IEmailProvider _provider;
            private readonly IOptions<DomainOptions> _options;

            public Handler(IDbContext db, UserManager<User> userManager, IEmailProvider provider, IOptions<DomainOptions> options)
            {
                _db = db;
                _userManager = userManager;
                _provider = provider;
                _options = options;
            }

            public async Task<Result<EventForumAnswer>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.QuestionId))
                    return Result.Fail<EventForumAnswer>("Id da Pergunta não informado");

                var question = await GetForumQuestion(request.QuestionId, cancellationToken);

                if (question == null)
                    return Result.Fail<EventForumAnswer>("Pergunta não encontrada");

                var checkString = await CheckForumDate(question.EventScheduleId, cancellationToken);
                if (!string.IsNullOrEmpty(checkString))
                    return Result.Fail<EventForumAnswer>(checkString);

                var answerResult = CreateAnswer(request);
                if (answerResult.IsFailure)
                    return Result.Fail<EventForumAnswer>(answerResult.Error);
                    
                await _db.EventForumAnswerCollection.InsertOneAsync(
                    answerResult.Data
                );

                await SendEmailToQuestioner(question, cancellationToken);

                await SendEmailToForumActiveUsers(question, answerResult.Data, cancellationToken);

                return Result.Ok( answerResult.Data );
            }

            private Result<EventForumAnswer> CreateAnswer(Contract request)
            {
                var questionId = ObjectId.Parse(request.QuestionId);

                return EventForumAnswer.Create(
                    questionId, request.User.Id,
                    request.User.Name, request.User.ImageUrl,
                    request.Text, new List<string>()
                );
            }

            private async Task<EventForumQuestion> GetForumQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);
                var query = await _db.Database
                    .GetCollection<EventForumQuestion>("EventForumQuestions")
                    .FindAsync(x => x.Id == questionId);

                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            }

            private async Task<bool> SendEmailToQuestioner(EventForumQuestion question, CancellationToken cancellationToken)
            {
                var userQry = await _db.UserCollection.FindAsync(u => u.Id == question.CreatedBy, cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                var eventQry = await _db.EventCollection.FindAsync(m => m.Id == question.EventId, cancellationToken: cancellationToken);
                var dbEvent = await eventQry.SingleOrDefaultAsync(cancellationToken);

                try
                {
                    if (user != null && dbEvent != null)
                    {
                        var emailData = new EmailUserData
                        {
                            Email = user.Email,
                            Name = user.Name,
                            ExtraData = new Dictionary<string, string>
                            {
                                { "name", user.UserName },
                                { "forumTitle", question.Title },
                                { "forumUrl", _options.Value.SiteUrl + $"/forum-evento/{dbEvent.Id}/{question.EventScheduleId}/{question.Id}" }
                            }
                        };
                        await _provider.SendEmail(emailData, "Responderam sua pergunta", "BTG-ForumQuestionAnswered");
                        await SaveNotification(user.Id, dbEvent, question.EventScheduleId, question.Id, true);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    await SaveNotification(user.Id, dbEvent, question.EventScheduleId, question.Id, false);
                    return false;
                }
            }
            private async Task<bool> SendEmailToForumActiveUsers(EventForumQuestion question, EventForumAnswer answer, CancellationToken cancellationToken)
            {

                var eventQry = await _db.EventCollection.FindAsync(m => m.Id == question.EventId, cancellationToken: cancellationToken);
                var dbEvent = await eventQry.SingleOrDefaultAsync(cancellationToken);

                var forumEmailUsers = _db.UserCollection.AsQueryable().Where(u => u.ForumActivities.HasValue && u.ForumActivities.Value).ToList();
                try
                {
                    if (forumEmailUsers != null && forumEmailUsers.Count > 0)
                    {
                        foreach (User user in forumEmailUsers)
                        {
                            if (user != null && (!string.IsNullOrEmpty(user.ForumEmail) || !string.IsNullOrEmpty(user.Email)))
                            {
                                var emailToSend = string.IsNullOrEmpty(user.ForumEmail) ? user.Email : user.ForumEmail;
                                var emailData = new EmailUserData
                                {
                                    Email = emailToSend,
                                    Name = user.Name,
                                    ExtraData = new Dictionary<string, string>
                                {
                                    { "name", user.UserName },
                                    { "username", answer.UserName },
                                    { "forumTitle", question.Title },
                                    { "forumUrl", _options.Value.SiteUrl + $"/forum-evento/{dbEvent.Id}/{question.EventScheduleId}/{question.Id}" }
                                }
                                };
                                await _provider.SendEmail(emailData, "Uma atividade no evento " + dbEvent.Title, "BTG-ForumActivity");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
                return true;
            }


            private async Task<bool> SaveNotification(ObjectId userId, Event eve, ObjectId eventScheduleId, ObjectId questionId, bool emailDelivered)
            {
                var path = "/forum-evento/" + eve.Id + "/" + eventScheduleId + "/" + questionId.ToString();

                var notification = Notification.Create(
                    userId, emailDelivered,
                    "Nova resposta para sua pergunta",
                    "Sua pergunta no fórum do evento " + eve.Title + " recebeu uma nova resposta. Confira!",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }

            private async Task<string> CheckForumDate(ObjectId eventScheduleId, CancellationToken cancellationToken)
            {
                var query = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x =>
                        x.Schedules.Any(y => y.Id == eventScheduleId),
                        cancellationToken: cancellationToken
                    );
                var eve = query.FirstOrDefault();
                if (eve != null)
                {
                    var evesche = eve.Schedules.Where(x => x.Id == eventScheduleId).FirstOrDefault();
                    if (evesche != null)
                    {
                        var today = DateTimeOffset.Now;
                        if (evesche.ForumStartDate > today || evesche.ForumEndDate < today)
                        {
                            return "Não é mais possivel participar deste fórum";
                        }
                        return null;
                    }
                    return "Evento não encontrado";
                }
                return "Evento não encontrado";
            }
        }
    }
}
