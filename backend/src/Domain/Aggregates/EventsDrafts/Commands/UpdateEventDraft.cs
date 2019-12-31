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
    public class UpdateEventDraft
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
            public PrepQuiz PrepQuiz { get; set; }
            public List<PrepQuizItem> PrepQuizQuestionList { get; set; }
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
        public class PrepQuizItem
        {
            public int QuestionId { get; set; }
            public string Question { get; set; }
            public bool FileAsAnswer { get; set; }
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
                if (request.UserRole == "Secretary")
                    return Result.Fail<Contract>("Acesso Negado");

                var evId = ObjectId.Parse(request.Id);

                var dbEvent = await _db.EventDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == evId || x.EventId == evId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

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

                var eventResult = Event.Create(
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
                    request.StoreUrl,
                    request.ForceProblemStatement
                );

                if (eventResult.IsFailure)
                    return Result.Fail<Contract>(eventResult.Error);

                var newEvent = eventResult.Data;
                newEvent.PrepQuizQuestionList = new List<PrepQuizQuestion>();

                if (request.PrepQuizQuestionList != null && request.PrepQuizQuestionList.Count > 0)
                {
                    var questions = request.PrepQuizQuestionList.Select(x => x.Question).ToArray();

                    for (int i = 0; i < request.PrepQuizQuestionList.Count; i++)
                    {
                        var prepQuizResult = PrepQuizQuestion.Create(request.PrepQuizQuestionList[i].Question, request.PrepQuizQuestionList[i].FileAsAnswer, questions);
                        if (prepQuizResult.IsFailure)
                            return Result.Fail<Contract>(prepQuizResult.Error);
                        newEvent.PrepQuizQuestionList.Add(prepQuizResult.Data);
                    }
                }

                var oldEventList = JsonConvert.SerializeObject(new List<EventDraft>
                {
                    dbEvent
                });

                if (dbEvent == null)
                {
                    var originalEvent = await GetEvent(request.Id);
                    if (originalEvent == null)
                        return Result.Fail<Contract>("Evento não existe");

                    dbEvent = await CreateEventDraft(
                        request, originalEvent, duration, videoDuration, cancellationToken
                    );
                }
                else
                {
                    dbEvent.Title = newEvent.Title;
                    dbEvent.Excerpt = newEvent.Excerpt;
                    dbEvent.ImageUrl = newEvent.ImageUrl;
                    dbEvent.InstructorId = newEvent.InstructorId;
                    dbEvent.Instructor = newEvent.Instructor;
                    dbEvent.InstructorMiniBio = newEvent.InstructorMiniBio;
                    dbEvent.InstructorImageUrl = newEvent.InstructorImageUrl;
                    dbEvent.Tags = newEvent.Tags;
                    dbEvent.VideoUrl = newEvent.VideoUrl;
                    dbEvent.VideoDuration = newEvent.VideoDuration;
                    dbEvent.Duration = newEvent.Duration;
                    dbEvent.PrepQuiz = newEvent.PrepQuiz;
                    dbEvent.CertificateUrl = newEvent.CertificateUrl;
                    dbEvent.TutorsIds = newEvent.TutorsIds;
                    dbEvent.StoreUrl = newEvent.StoreUrl;
                    dbEvent.CreateInEcommerce = request.CreateInEcommerce.HasValue ? request.CreateInEcommerce.Value : false;
                    dbEvent.EcommerceId = request.EcommerceId;
                    dbEvent.ForceProblemStatement = request.ForceProblemStatement;
                    dbEvent.PrepQuizQuestionList = newEvent.PrepQuizQuestionList;

                    await _db.EventDraftCollection.ReplaceOneAsync(t =>
                        t.Id == dbEvent.Id, dbEvent,
                        cancellationToken: cancellationToken
                    );

                    var newEventList = new List<EventDraft>
                    {
                        dbEvent
                    };

                    var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), dbEvent.Id, dbEvent.GetType().ToString(),
                    JsonConvert.SerializeObject(newEventList), EntityAction.Update, oldEventList);

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);


                }

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

                var eventId = ObjectId.Parse(request.Id);

                var draft = EventDraft.Create(
                    eventId, request.Title, request.Excerpt, request.ImageUrl,
                    instructorId, request.Instructor, request.InstructorMiniBio, request.InstructorImageUrl,
                    request.Tags, request.VideoUrl, videoDuration, duration,
                    request.CertificateUrl, tutorsIds, request.StoreUrl
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
