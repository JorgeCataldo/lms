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
    public class SaveForumQuestionCommand
    {
        public class Contract : CommandContract<Result<ForumQuestion>>
        {
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string ContentId { get; set; }
            public string ContentName { get; set; }
            public string ModuleName { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Position { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ForumQuestion>>
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

            public async Task<Result<ForumQuestion>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<ForumQuestion>("Id do Módulo não informado");

                var questionResult = CreateQuestion(request);
                if (questionResult.IsFailure)
                    return Result.Fail<ForumQuestion>(questionResult.Error);

                var forumQuestion = questionResult.Data;
                await _db.ForumQuestionCollection.InsertOneAsync(forumQuestion);

                var moduleId = ObjectId.Parse(request.ModuleId);
                var forum = await GetForum(moduleId, cancellationToken);

                if (forum == null)
                {
                    var newForum = Forum.Create(
                        moduleId,
                        request.ModuleName,
                        new List<ObjectId>() { forumQuestion.Id }
                    );

                    if (newForum.IsFailure)
                        return Result.Fail<ForumQuestion>(newForum.Error);

                    forum = newForum.Data;
                    await _db.ForumCollection.InsertOneAsync(forum);
                }
                else
                {
                    forum.Questions.Add(forumQuestion.Id);

                    await _db.ForumCollection.ReplaceOneAsync(f =>
                        f.Id == forum.Id, forum,
                        cancellationToken: cancellationToken
                    );
                }
                await SendEmailModuleInstructor(forumQuestion, cancellationToken);

                return Result.Ok( forumQuestion );
            }

            private Result<ForumQuestion> CreateQuestion(Contract request)
            {
                var moduleId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);

                ObjectId? subjectId = null;
                if (!String.IsNullOrEmpty(request.SubjectId) && !String.IsNullOrEmpty(request.SubjectName))
                    subjectId = ObjectId.Parse(request.SubjectId);

                ObjectId? contentId = null;
                if (!String.IsNullOrEmpty(request.ContentId) && !String.IsNullOrEmpty(request.ContentName))
                    contentId = ObjectId.Parse(request.ContentId);

                return ForumQuestion.Create(
                    moduleId, userId,
                    request.Title, request.Description, new List<string>(),
                    subjectId, request.SubjectName,
                    contentId, request.ContentName,
                    request.Position
                );
            }

            private async Task<Forum> GetForum(ObjectId moduleId, CancellationToken cancellationToken)
            {
                var query = await _db.Database
                    .GetCollection<Forum>("Forums")
                    .FindAsync(
                        x => x.ModuleId == moduleId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private async Task<bool> SendEmailModuleInstructor(ForumQuestion forumQuestion, CancellationToken cancellationToken)
            {
                var moduleQry = await _db.ModuleCollection.FindAsync(m => m.Id == forumQuestion.ModuleId, cancellationToken: cancellationToken);
                var module = await moduleQry.SingleOrDefaultAsync(cancellationToken);

                if (module != null && module.InstructorId != null && module.InstructorId != ObjectId.Empty)
                {
                    var userQry = await _db.UserCollection.FindAsync(u => u.Id == module.InstructorId, cancellationToken: cancellationToken);
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
                                    { "name", user.Name },
                                    { "forumTitle", forumQuestion.Title },
                                    { "moduleName", module.Title },
                                    { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{forumQuestion.ModuleId}/{forumQuestion.Id}" }
                                }
                            };
                            await _provider.SendEmail(emailData, "Uma pergunta foi feita no modulo que você é o instrutor", "BTG-ForumQuestionInstructor");
                            await SaveNotification(user.Id, module, forumQuestion.Id, true, "Nova pergunta no modulo que você é o instrutor");
                        }
                    }
                    catch (Exception ex)
                    {
                        await SaveNotification(user.Id, module, forumQuestion.Id, false, "Nova pergunta no modulo que você é o instrutor");
                    }
                }

                if (module != null && module.TutorsIds != null && module.TutorsIds.Count > 0)
                {
                    foreach (ObjectId tutorId in module.TutorsIds)
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
                                            { "name", user.Name },
                                            { "forumTitle", forumQuestion.Title },
                                            { "moduleName", module.Title },
                                            { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{forumQuestion.ModuleId}/{forumQuestion.Id}" }
                                        }
                                    };
                                    await _provider.SendEmail(emailData, "Uma pergunta foi feita no modulo que você é o tutor", "BTG-ForumQuestionInstructor");
                                    await SaveNotification(user.Id, module, forumQuestion.Id, true, "Nova pergunta no modulo que você é o tutor");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SaveNotification(user.Id, module, forumQuestion.Id, false, "Nova pergunta no modulo que você é o tutor");
                            }
                        }
                    }
                }

                if (module != null && module.ExtraInstructorIds != null && module.ExtraInstructorIds.Count > 0)
                {
                    foreach (ObjectId InstructorId in module.ExtraInstructorIds)
                    {
                        if (InstructorId != null && InstructorId != ObjectId.Empty)
                        {
                            var userQry = await _db.UserCollection.FindAsync(u => u.Id == InstructorId, cancellationToken: cancellationToken);
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
                                            { "name", user.Name },
                                            { "forumTitle", forumQuestion.Title },
                                            { "moduleName", module.Title },
                                            { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{forumQuestion.ModuleId}/{forumQuestion.Id}" }
                                        }
                                    };
                                    await _provider.SendEmail(emailData, "Uma pergunta foi feita no modulo que você é o tutor", "BTG-ForumQuestionInstructor");
                                    await SaveNotification(user.Id, module, forumQuestion.Id, true, "Nova pergunta no modulo que você é o tutor");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SaveNotification(user.Id, module, forumQuestion.Id, false, "Nova pergunta no modulo que você é o tutor");
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
                                        { "forumUrl", _options.Value.SiteUrl + $"/forum/{module.Title}/{forumQuestion.ModuleId}/{forumQuestion.Id}" }
                                    }
                                    };
                                    await _provider.SendEmail(emailData, "Uma atividade no módulo " + module.Title, "BTG-ForumActivity");
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

            private async Task<bool> SaveNotification(ObjectId userId, Module module, ObjectId questionId, bool emailDelivered, string text)
            {
                var path = "/forum/" + module.Title + "/" + module.Id + "/" + questionId.ToString();

                var notification = Notification.Create(
                    userId, emailDelivered,
                    text,
                    "O fórum do módulo " + module.Title + " recebeu uma nova pergunta. Confira!",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
