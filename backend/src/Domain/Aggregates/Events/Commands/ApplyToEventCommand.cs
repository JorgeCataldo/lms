using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ApplyToEventCommand
    {
        public class Contract : CommandContract<Result<ApplicationStatus>>
        {
            public string EventId { get; set; }
            public string ScheduleId { get; set; }
            public string UserId { get; set; }
            public List<string> PrepQuizAnswers { get; set; }
            public List<PrepQuizAnswerItem> PrepQuizAnswersList { get; set; }
        }

        public class PrepQuizAnswerItem
        {
            public string Answer { get; set; }
            public bool FileAsAnswer { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string LineManager { get; set; }
            public string UserName { get; set; }
            public string RegistrationId { get; set; }
            public User.RelationalItem Rank { get; set; }
            public List<User.UserProgress> TracksInfo { get; set; }
            public List<User.UserProgress> ModulesInfo { get; set; }
            public DateTimeOffset? BirthDate { get; set; }
            public User.RelationalItem Location { get; set; }
            public bool IsFavorite { get; set; } = false;
        }

        public class Handler : IRequestHandler<Contract, Result<ApplicationStatus>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<ApplicationStatus>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var eventId = ObjectId.Parse(request.EventId);
                var userId = ObjectId.Parse(request.UserId);
                var scheduleId = ObjectId.Parse(request.ScheduleId);

                //VER QUERY COM O PEDRO
                //var query  = await _db.UserCollection
                //    .AsQueryable()
                //    .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken);

                var eventApplication = await
                    (await _db.Database.GetCollection<EventApplication>("EventApplications")
                        .FindAsync(x => x.EventId == eventId && x.UserId == userId && x.ScheduleId == scheduleId,
                        cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (eventApplication != null)
                {
                    return Result.Fail<ApplicationStatus>("Inscrição já existente");
                }

                var evt = await (await _db
                    .Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == eventId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return Result.Fail<ApplicationStatus>("Evento não Encontrado");

                var schedule = evt.Schedules.FirstOrDefault(x => x.Id == scheduleId);
                if (schedule == null)
                    return Result.Fail<ApplicationStatus>("Evento não Encontrado");

                var userModuleProgress = await (await _db.Database
                    .GetCollection<UserModuleProgress>("UserModuleProgress")
                    .FindAsync(x => x.UserId == userId, cancellationToken: cancellationToken))
                    .ToListAsync(cancellationToken);

                //foreach (var evtRequirement in evt.Requirements.Where(x => !x.Optional))
                //{
                //    var userProgress = userModuleProgress.FirstOrDefault(x => x.ModuleId == evtRequirement.ModuleId);
                //    if (userProgress == null || userProgress.Level <= evtRequirement.RequirementValue.Level)
                //        return Result.Fail<ApplicationStatus>("Pré-Requisitos não atendidos");
                //}

                var prepQuizAnswers = new List<PrepQuizAnswer>();
                for (int i = 0; i < request.PrepQuizAnswersList.Count; i++)
                {
                    var quizAnswer = PrepQuizAnswer.Create(request.PrepQuizAnswersList[i].Answer, request.PrepQuizAnswersList[i].FileAsAnswer);

                    if (quizAnswer.IsFailure)
                        return Result.Fail<ApplicationStatus>(quizAnswer.Error);

                    prepQuizAnswers.Add(quizAnswer.Data);
                }

                if(evt.PrepQuiz == null && evt.PrepQuizQuestionList.Count > 0)
                {
                    evt.PrepQuiz = PrepQuiz.Create(evt.PrepQuizQuestionList.Select(x => x.Question).ToArray()).Data;
                }

                await _db.EventApplicationCollection.InsertOneAsync(new EventApplication()
                {
                    ApplicationStatus = ApplicationStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    EventId = eventId,
                    EventDate = schedule.EventDate,
                    UserId = userId,
                    ScheduleId = scheduleId,
                    PrepQuiz = evt.PrepQuiz,
                    PrepQuizAnswers = request.PrepQuizAnswers,
                    PrepQuizAnswersList = prepQuizAnswers,
                    RequestedDate = DateTimeOffset.UtcNow,
                    ResolutionDate = null,
                }, cancellationToken: cancellationToken);

                return Result.Ok(ApplicationStatus.Pending);
            }
        }
    }
}
