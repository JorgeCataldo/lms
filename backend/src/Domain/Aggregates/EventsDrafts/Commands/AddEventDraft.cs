using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class AddEventDraft
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public int? Duration { get; set; }
            public string[] Tags { get; set; }
            public string[] PrepQuizQuestions { get; set; }
            public string CertificateUrl { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public List<string> TutorsIds { get; set; }
            public string StoreUrl { get; set; }
            public bool? CreateInEcommerce { get; set; } = false;
            public long? EcommerceId { get; set; }
            public bool? ForceProblemStatement { get; set; } = false;
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IMediator mediator, IConfiguration configuration)
            {
                _db = db;
                _mediator = mediator;
                _configuration = configuration;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                Duration duration = null;
                if (request.Duration.HasValue)
                {
                    var durationResult = Duration.Create(request.Duration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail<Contract>(durationResult.Error);

                    duration = durationResult.Data;
                }

                Duration videoDuration = null;
                if (request.VideoDuration.HasValue)
                {
                    var durationResult = Duration.Create(request.VideoDuration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail<Contract>(durationResult.Error);

                    videoDuration = durationResult.Data;
                }

                ObjectId instructorId = String.IsNullOrEmpty(request.InstructorId) ?
                    ObjectId.Empty : ObjectId.Parse(request.InstructorId);

                var tutorsIds = request.TutorsIds != null ?
                    request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList() :
                    new List<ObjectId>();

                Event dbEvent = null;

                if (!String.IsNullOrEmpty(request.Id))
                    dbEvent = await GetEvent(request.Id);
                
                if (dbEvent == null)
                {
                    dbEvent = Event.Create(
                        request.Title,
                        request.Excerpt,
                        request.ImageUrl,
                        instructorId,
                        request.Instructor,
                        request.InstructorMiniBio,
                        request.InstructorImageUrl,
                        request.Tags,
                        request.VideoUrl,
                        request.VideoDuration,
                        request.Duration,
                        request.CertificateUrl,
                        tutorsIds,
                        request.StoreUrl
                    ).Data;

                    await _db.EventCollection.InsertOneAsync(
                        dbEvent, cancellationToken: cancellationToken
                    );

                    var newEventList = new List<Event>
                    {
                        dbEvent
                    };

                    var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), dbEvent.Id, dbEvent.GetType().ToString(),
                    JsonConvert.SerializeObject(newEventList), EntityAction.Add, "");

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);
                }

                var newEvent = await CreateEventDraft(request, dbEvent, duration, videoDuration, cancellationToken);

                request.Id = newEvent.Id.ToString();

                return Result.Ok(request);
            }

            private async Task<EventDraft> CreateEventDraft(
                Contract request, Event originalEvent, Duration duration, Duration videoDuration, CancellationToken token
            ) {
                var tutorsIds = request.TutorsIds != null ?
                    request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList() :
                    new List<ObjectId>();

                var instructorId = String.IsNullOrEmpty(request.InstructorId) ?
                    ObjectId.Empty : ObjectId.Parse(request.InstructorId);
                
                var draft = EventDraft.Create(
                    originalEvent.Id, request.Title, request.Excerpt, request.ImageUrl,
                    instructorId, request.Instructor, request.InstructorMiniBio, request.InstructorImageUrl,
                    request.Tags, request.VideoUrl, videoDuration, duration,
                    request.CertificateUrl, tutorsIds, request.StoreUrl, request.ForceProblemStatement
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
