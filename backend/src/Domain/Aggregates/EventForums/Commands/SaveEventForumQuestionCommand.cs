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
    public class SaveEventForumQuestionCommand
    {
        public class Contract : CommandContract<Result<EventForumQuestion>>
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string EventName { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Position { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<EventForumQuestion>>
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

            public async Task<Result<EventForumQuestion>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.EventScheduleId))
                    return Result.Fail<EventForumQuestion>("Id do Evento não informado");

                var eventId = ObjectId.Parse(request.EventId);
                var eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                var checkString = await CheckForumDate(eventScheduleId, cancellationToken);
                if (!string.IsNullOrEmpty(checkString))
                    return Result.Fail<EventForumQuestion>(checkString);

                var questionResult = CreateQuestion(request);
                if (questionResult.IsFailure)
                    return Result.Fail<EventForumQuestion>(questionResult.Error);

                var forumQuestion = questionResult.Data;
                await _db.EventForumQuestionCollection.InsertOneAsync(forumQuestion);

                var forum = await GetForum(eventScheduleId, cancellationToken);

                if (forum == null)
                {
                    var newForum = EventForum.Create(
                        eventId,
                        eventScheduleId,
                        request.EventName,
                        new List<ObjectId>() { forumQuestion.Id }
                    );

                    if (newForum.IsFailure)
                        return Result.Fail<EventForumQuestion>(newForum.Error);

                    forum = newForum.Data;
                    await _db.EventForumCollection.InsertOneAsync(forum);
                }
                else
                {
                    forum.Questions.Add(forumQuestion.Id);

                    await _db.EventForumCollection.ReplaceOneAsync(f =>
                        f.Id == forum.Id, forum,
                        cancellationToken: cancellationToken
                    );
                }
                await SendEmailEventInstructor(forumQuestion, cancellationToken);

                return Result.Ok( forumQuestion );
            }

            private Result<EventForumQuestion> CreateQuestion(Contract request)
            {
                var eventId = ObjectId.Parse(request.EventId);
                var eventScheduleId = ObjectId.Parse(request.EventScheduleId);
                var userId = ObjectId.Parse(request.UserId);

                return EventForumQuestion.Create(
                    eventId, eventScheduleId, userId,
                    request.Title, request.Description, new List<string>(),
                    request.Position
                );
            }

            private async Task<EventForum> GetForum(ObjectId eventScheduleId, CancellationToken cancellationToken)
            {
                var query = await _db.Database
                    .GetCollection<EventForum>("EventForums")
                    .FindAsync(
                        x => x.EventScheduleId == eventScheduleId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private async Task<bool> SendEmailEventInstructor(EventForumQuestion forumQuestion, CancellationToken cancellationToken)
            {
                var eventQry = await _db.EventCollection.FindAsync(m => m.Id == forumQuestion.EventId, cancellationToken: cancellationToken);
                var dbEvent = await eventQry.SingleOrDefaultAsync(cancellationToken);

                if (dbEvent != null && dbEvent.InstructorId != null && dbEvent.InstructorId != ObjectId.Empty)
                {
                    var userQry = await _db.UserCollection.FindAsync(u => u.Id == dbEvent.InstructorId, cancellationToken: cancellationToken);
                    var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                    try
                    {
                        if (user != null)
                        {
                            var emailData = new EmailUserData
                            {
                                Email = user.Email,
                                Name = user.Name,
                                ExtraData = new Dictionary<string, string>
                                {
                                    { "name", user.UserName },
                                    { "forumTitle", forumQuestion.Title },
                                    { "moduleName", dbEvent.Title },
                                    { "forumUrl", _options.Value.SiteUrl + $"/forum-evento/{dbEvent.Id}/{forumQuestion.EventScheduleId}/{forumQuestion.Id}" }
                                }
                            };
                            await _provider.SendEmail(emailData, "Uma pergunta foi feita no evento que você é o instrutor", "BTG-ForumQuestionInstructor");
                            await SaveNotification(user.Id, dbEvent, forumQuestion.EventScheduleId, forumQuestion.Id, true, "Nova pergunta no evento que você é o instrutor");
                        }
                    }
                    catch (Exception ex)
                    {
                        await SaveNotification(user.Id, dbEvent, forumQuestion.EventScheduleId, forumQuestion.Id, false, "Nova pergunta no evento que você é o instrutor");
                    }
                }

                if (dbEvent != null && dbEvent.TutorsIds != null && dbEvent.TutorsIds.Count > 0)
                {
                    foreach (ObjectId tutorId in dbEvent.TutorsIds)
                    {
                        if (tutorId != null && tutorId != ObjectId.Empty)
                        {
                            var userQry = await _db.UserCollection.FindAsync(u => u.Id == tutorId, cancellationToken: cancellationToken);
                            var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                            try
                            {
                                if (user != null)
                                {
                                    var emailData = new EmailUserData
                                    {
                                        Email = user.Email,
                                        Name = user.Name,
                                        ExtraData = new Dictionary<string, string>
                                        {
                                            { "name", user.UserName },
                                            { "forumTitle", forumQuestion.Title },
                                            { "moduleName", dbEvent.Title },
                                            { "forumUrl", _options.Value.SiteUrl + $"/forum-evento/{dbEvent.Id}/{forumQuestion.EventScheduleId}/{forumQuestion.Id}" }
                                        }
                                    };
                                    await _provider.SendEmail(emailData, "Uma pergunta foi feita no evento que você é o tutor", "BTG-ForumQuestionInstructor");
                                    await SaveNotification(user.Id, dbEvent, forumQuestion.EventScheduleId, forumQuestion.Id, true, "Nova pergunta no evento que você é o tutor");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SaveNotification(user.Id, dbEvent, forumQuestion.EventScheduleId, forumQuestion.Id, false, "Nova pergunta no evento que você é o tutor");
                            }
                        }
                    }
                }

                var forumEmailUsers = _db.UserCollection.AsQueryable().Where(u => u.ForumActivities.HasValue && u.ForumActivities.Value).ToList();
                try
                {
                    if (forumEmailUsers != null && forumEmailUsers.Count > 0)
                    {
                        var activeUser = _db.UserCollection.AsQueryable().FirstOrDefault(u => u.Id == forumQuestion.CreatedBy);
                        if (activeUser != null)
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
                                        { "username", activeUser.Name },
                                        { "forumTitle", forumQuestion.Title },
                                        { "forumUrl", _options.Value.SiteUrl + $"/forum-evento/{dbEvent.Id}/{forumQuestion.EventScheduleId}/{forumQuestion.Id}" }
                                    }
                                    };
                                    await _provider.SendEmail(emailData, "Uma atividade no evento " + dbEvent.Title, "BTG-ForumActivity");
                                }
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

            private async Task<bool> SaveNotification(ObjectId userId, Event eve, ObjectId eventScheduleId, ObjectId questionId, bool emailDelivered, string text)
            {
                var path = "/forum-evento/" + eve.Id + "/" + eventScheduleId + "/" + questionId.ToString();

                var notification = Notification.Create(
                    userId, emailDelivered,
                    text,
                    "O fórum do evento " + eve.Title + " recebeu uma nova pergunta. Confira!",
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
