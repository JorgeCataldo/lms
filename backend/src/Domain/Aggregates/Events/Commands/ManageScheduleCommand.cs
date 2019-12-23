using MediatR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Domain.Aggregates.Locations;
using Domain.Base;

namespace Domain.Aggregates.Events.Commands
{
    public class ManageScheduleCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string EventId { get; set; }
            public string Id { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public DateTimeOffset SubscriptionStartDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
            public DateTimeOffset? ForumStartDate { get; set; }
            public DateTimeOffset? ForumEndDate { get; set; }
            public int Duration { get; set; }
            public bool Published { get; set; }
            public string WebinarUrl { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public string Location { get; set; }
            public int? ApplicationLimit { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                var mId = ObjectId.Parse(request.EventId);
                var evt = await (await _db
                    .Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                //var location = request.Location == null ? null : await _db.LocationCollection
                //        .AsQueryable()
                //        .Where(x => x.Id == ObjectId.Parse(request.Location))
                //        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                var location = await (await _db
                    .Database
                    .GetCollection<Location>("Locations")
                    .FindAsync(x => x.Id == ObjectId.Parse(request.Location)))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);


                if (evt == null)
                    return Result.Fail<Contract>("Evento não Encontrado");
                
                var oldValuesJson = JsonConvert.SerializeObject(evt.Schedules);

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!evt.InstructorId.HasValue || evt.InstructorId.Value != userId)
                        return Result.Fail<Contract>("Acesso Negado");
                }

                var scheduleResponse = EventSchedule.Create(
                    request.EventDate, request.SubscriptionStartDate,
                    request.SubscriptionEndDate, request.Duration, request.Published,
                    request.WebinarUrl, request.ForumStartDate, request.ForumEndDate, request.ApplicationLimit, location
                );

                if (scheduleResponse.IsFailure)
                    return Result.Fail<Contract>(scheduleResponse.Error);

                evt.Schedules = evt.Schedules ?? new List<EventSchedule>();
                if (string.IsNullOrEmpty(request.Id))
                {
                    evt.Schedules.Add(scheduleResponse.Data);
                    request.Id = scheduleResponse.Data.Id.ToString();
                }
                else
                {
                    var newSchedule = scheduleResponse.Data;
                    var schedule =
                        evt.Schedules.FirstOrDefault(x => x.Id == ObjectId.Parse(request.Id));
                    if (schedule == null)
                        return Result.Fail<Contract>("Programação de evento não encontrada");

                    schedule.Duration = newSchedule.Duration;
                    schedule.EventDate = newSchedule.EventDate;
                    schedule.SubscriptionStartDate = newSchedule.SubscriptionStartDate;
                    schedule.SubscriptionEndDate = newSchedule.SubscriptionEndDate;
                    schedule.ForumStartDate = newSchedule.ForumStartDate;
                    schedule.ForumEndDate = newSchedule.ForumEndDate;
                    schedule.Published = newSchedule.Published;
                    schedule.WebinarUrl = newSchedule.WebinarUrl;
                    schedule.Location = newSchedule.Location;
                }
                
                await _db.EventCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );


                var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), evt.Id, evt.Schedules.GetType().ToString(),
                JsonConvert.SerializeObject(evt.Schedules), EntityAction.Update, oldValuesJson);

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                return Result.Ok(request);
            }
        }
    }
}
