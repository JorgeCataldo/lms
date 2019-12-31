using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Commands
{
    public class SaveAnswerCommand
    {
        public class Contract : CommandContract<Result<ForumAnswer>>
        {
            public string QuestionId { get; set; }
            public string Text { get; set; }
            public User User { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ForumAnswer>>
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

            public async Task<Result<ForumAnswer>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.QuestionId))
                    return Result.Fail<ForumAnswer>("Id da Pergunta não informado");

                var question = await GetForumQuestion(request.QuestionId, cancellationToken);

                if (question == null)
                    return Result.Fail<ForumAnswer>("Pergunta não encontrada");

                var answerResult = CreateAnswer(request);
                if (answerResult.IsFailure)
                    return Result.Fail<ForumAnswer>(answerResult.Error);
                    
                await _db.ForumAnswerCollection.InsertOneAsync(
                    answerResult.Data
                );

                await SendEmailToQuestioner(question, cancellationToken);

                await SendEmailToForumActiveUsers(question, answerResult.Data, cancellationToken);

                return Result.Ok( answerResult.Data );
            }

            private Result<ForumAnswer> CreateAnswer(Contract request)
            {
                var questionId = ObjectId.Parse(request.QuestionId);

                return ForumAnswer.Create(
                    questionId, request.User.Id,
                    request.User.Name, request.User.ImageUrl,
                    request.Text, new List<string>()
                );
            }

            private async Task<ForumQuestion> GetForumQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);
                var query = await _db.Database
                    .GetCollection<ForumQuestion>("ForumQuestions")
                    .FindAsync(x => x.Id == questionId);

                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            }

            private async Task<bool> SendEmailToQuestioner(ForumQuestion question, CancellationToken cancellationToken)
            {
                var userQry = await _db.UserCollection.FindAsync(u => u.Id == question.CreatedBy, cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                var moduleQry = await _db.ModuleCollection.FindAsync(m => m.Id == question.ModuleId, cancellationToken: cancellationToken);
                var module = await moduleQry.SingleOrDefaultAsync(cancellationToken);

                try
                {
                    if (user != null && module != null)
                    {
                        var emailData = new EmailUserData
                        {
                            Email = user.Email,
                            Name = user.Name,
                            ExtraData = new Dictionary<string, string>
                            {
                                { "name", user.Name },
                                { "forumTitle", question.Title },
                                { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{question.ModuleId}/{question.Id}" }
                            }
                        };
                        await _provider.SendEmail(emailData, "Responderam sua pergunta", "BTG-ForumQuestionAnswered");
                        await SaveNotification(user.Id, module, question.Id, true);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    await SaveNotification(user.Id, module, question.Id, false);
                    return false;
                }
            }

            private async Task<bool> SaveNotification(ObjectId userId, Module module, ObjectId questionId, bool emailDelivered)
            {
                var path = "/forum/" + module.Title + "/" + module.Id + "/" + questionId.ToString();

                var notification = Notification.Create(
                    userId, emailDelivered,
                    "Nova resposta para sua pergunta",
                    "Sua pergunta no fórum do módulo " + module.Title + " recebeu uma nova resposta. Confira!",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }

            private async Task<bool> SendEmailToForumActiveUsers(ForumQuestion question, ForumAnswer answer, CancellationToken cancellationToken)
            {

                var moduleQry = await _db.ModuleCollection.FindAsync(m => m.Id == question.ModuleId, cancellationToken: cancellationToken);
                var module = await moduleQry.SingleOrDefaultAsync(cancellationToken);

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
                                    { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{question.ModuleId}/{question.Id}" }
                                }
                                };
                                await _provider.SendEmail(emailData, "Uma atividade no módulo " + module.Title, "BTG-ForumActivity");
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
        }
    }
}
