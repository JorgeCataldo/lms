using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;
using SupportMaterial = Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class ManageDraftSupportMaterials
    {
        public class Contract : CommandContract<Result<List<ContractSupportMaterials>>>
        {
            public string EventId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractSupportMaterials> SupportMaterials { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractSupportMaterials
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public int Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ContractSupportMaterials>>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<List<ContractSupportMaterials>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<List<ContractSupportMaterials>>("Acesso Negado");

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
                        return Result.Fail<List<ContractSupportMaterials>>("Evento não existe");

                    evt = await CreateEventDraft(
                        request, originalEvent, cancellationToken
                    );
                }

                evt.SupportMaterials = evt.SupportMaterials ?? new List<SupportMaterial>();

                if (request.DeleteNonExistent)
                {
                    var existingIds = from o in request.SupportMaterials
                                      where !string.IsNullOrEmpty(o.Id)
                                      select ObjectId.Parse(o.Id);

                    var include = from c in evt.SupportMaterials
                                  where existingIds.Contains(c.Id)
                                  select c;

                    evt.SupportMaterials = include.ToList();
                }

                foreach (var requestObj in request.SupportMaterials)
                {
                    SupportMaterial newObject = null;
                    if (string.IsNullOrEmpty(requestObj.Id))
                    {
                        var result = Create(
                            requestObj.Title,
                            requestObj.Description,
                            requestObj.DownloadLink,
                            (SupportMaterialTypeEnum)requestObj.Type
                        );
                        if (result.IsFailure)
                        {
                            return Result.Fail<List<ContractSupportMaterials>>(result.Error);
                        }

                        newObject = result.Data;
                        newObject.UpdatedAt = DateTimeOffset.UtcNow;
                        evt.SupportMaterials.Add(newObject);
                    }
                    else
                    {
                        newObject = evt.SupportMaterials.FirstOrDefault(x => x.Id == ObjectId.Parse(requestObj.Id));
                        if (newObject == null)
                        {
                            return Result.Fail<List<ContractSupportMaterials>>($"Material de suporte não encontrado ({requestObj.Id} - {requestObj.Title})");
                        }

                        newObject.Title = requestObj.Title;
                        newObject.Description = requestObj.Description;
                        newObject.DownloadLink = requestObj.DownloadLink;
                    }
                }

                await _db.EventDraftCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request.SupportMaterials);
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