using MediatR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using MongoDB.Driver.Linq;
using Domain.Aggregates.Events;
using System.Linq;
using Performance.Domain.Aggregates.AuditLogs;
using Newtonsoft.Json;
using Domain.Base;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class ManageDraftSchedule
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
                if (request.UserRole == "Secretary")
                    return Result.Fail<Contract>("Acesso Negado");

                var evtId = ObjectId.Parse(request.EventId);

                var evt = await _db.EventDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == evtId || x.EventId == evtId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                var location = string.IsNullOrEmpty(request.Location) ? null : await _db.LocationCollection.AsQueryable()
                    .Where(x => x.Id == ObjectId.Parse(request.Location))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                {
                    var originalEvent = await GetEvent(request.EventId);
                    if (originalEvent == null)
                        return Result.Fail<Contract>("Evento não existe");

                    evt = await CreateEventDraft(
                        request, originalEvent, cancellationToken
                    );
                }

                var scheduleResponse = EventSchedule.Create(
                    request.EventDate, request.SubscriptionStartDate,
                    request.SubscriptionEndDate, request.Duration, request.Published,
                    request.WebinarUrl, request.ForumStartDate, request.ForumEndDate, request.ApplicationLimit, location
                );

                if (scheduleResponse.IsFailure)
                    return Result.Fail<Contract>(scheduleResponse.Error);

                evt.Schedules = evt.Schedules ?? new List<EventSchedule>();

                var oldScheduleList = evt.Schedules;

                if (string.IsNullOrEmpty(request.Id))
                {
                    evt.Schedules.Add(scheduleResponse.Data);

                    var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), scheduleResponse.Data.Id, scheduleResponse.Data.GetType().ToString(),
                    "", !string.IsNullOrEmpty(request.Id) ? EntityAction.Update : EntityAction.Add, JsonConvert.SerializeObject(scheduleResponse.Data));

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);


                    request.Id = scheduleResponse.Data.Id.ToString();
                }
                else
                {
                    var newSchedule = scheduleResponse.Data;
                    var schedule = evt.Schedules.FirstOrDefault(x => x.Id == ObjectId.Parse(request.Id));
                    if (schedule == null)
                        return Result.Fail<Contract>("Programação de evento não encontrada");

                    var oldSchedule = new List<EventSchedule>
                {
                    schedule
                };

                    schedule.Duration = newSchedule.Duration;
                    schedule.EventDate = newSchedule.EventDate;
                    schedule.SubscriptionStartDate = newSchedule.SubscriptionStartDate;
                    schedule.SubscriptionEndDate = newSchedule.SubscriptionEndDate;
                    schedule.ForumStartDate = newSchedule.ForumStartDate;
                    schedule.ForumEndDate = newSchedule.ForumEndDate;
                    schedule.Published = newSchedule.Published;
                    schedule.WebinarUrl = newSchedule.WebinarUrl;
                    schedule.Location = newSchedule.Location;

                    var newScheduleJson = new List<EventSchedule>
                {
                    schedule
                };

                    var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), schedule.Id, schedule.GetType().ToString(),
                    JsonConvert.SerializeObject(oldSchedule), !string.IsNullOrEmpty(request.Id) ? EntityAction.Update : EntityAction.Add, JsonConvert.SerializeObject(newScheduleJson));

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);
                }

                await _db.EventDraftCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );


                return Result.Ok(request);
            }

            private async Task<EventDraft> CreateEventDraft(
                Contract request, Event originalEvent, CancellationToken token
            ) {
                var draft = EventDraft.Create(
                    originalEvent.Id, originalEvent.Title, originalEvent.Excerpt, originalEvent.ImageUrl,
                    originalEvent.InstructorId, originalEvent.Instructor, originalEvent.InstructorMiniBio, originalEvent.InstructorImageUrl,
                    originalEvent.Tags.Select(t => t.Name).ToArray(),
                    originalEvent.VideoUrl, originalEvent.VideoDuration, originalEvent.Duration,
                    originalEvent.CertificateUrl, originalEvent.TutorsIds, originalEvent.StoreUrl
                ).Data;

                draft.Schedules = originalEvent.Schedules;
                draft.Requirements = originalEvent.Requirements;
                draft.SupportMaterials = originalEvent.SupportMaterials;
                draft.PrepQuiz = originalEvent.PrepQuiz;

                await _db.EventDraftCollection.InsertOneAsync(
                    draft, cancellationToken: token
                );

                return draft;
            }

            private async Task<Event> GetEvent(string eventId)
            {
                var dbEventId = ObjectId.Parse(eventId);

                return await _db.EventCollection.AsQueryable()
                    .Where(ev => ev.Id == dbEventId)
                    .FirstOrDefaultAsync();
            }
        }
    }
}
