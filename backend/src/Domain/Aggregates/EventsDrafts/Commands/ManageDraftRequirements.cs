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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class ManageDraftRequirements
    {

        public class Contract : CommandContract<Result>
        {
            public string EventId { get; set; }
            public List<ContractRequirements> Requirements { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractRequirements
        {
            public string ModuleId { get; set; }
            public bool Optional { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail("Acesso Negado");

                var evtId = ObjectId.Parse(request.EventId);

                var evt = await _db.EventDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == evtId || x.EventId == evtId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                {
                    var originalEvent = await GetEvent(request.EventId);
                    if (originalEvent == null)
                        return Result.Fail("Evento não existe");

                    evt = await CreateEventDraft(
                        request, originalEvent, cancellationToken
                    );
                }

                var results = request.Requirements.Select(x =>
                    Requirement.Create(
                        ObjectId.Parse(x.ModuleId),
                        x.Optional,
                        x.Level,
                        x.Percentage,
                        ProgressType.ModuleProgress
                    )
                ).ToArray();

                var combinedResults = Result.Combine(results);
                if (combinedResults.IsFailure)
                    return Result.Fail(combinedResults.Error);

                evt.Requirements = results.Select(x => x.Data).ToList();

                await _db.EventDraftCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
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
