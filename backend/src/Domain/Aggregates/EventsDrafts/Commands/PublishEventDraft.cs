using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Tracks;
using Domain.Base;
using Domain.Data;
using Domain.ECommerceIntegration.Woocommerce;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class PublishEventDraft
    {
        public class Contract : CommandContract<Result>
        {
            public string EventId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
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

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.EventId))
                    return Result.Fail("Acesso Negado");

                var draftId = ObjectId.Parse(request.EventId);

                var draft = await _db.EventDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == draftId || x.EventId == draftId
                        )
                    )
                    .FirstOrDefaultAsync();

                if (draft == null)
                    return Result.Fail("Não há rascunho para ser publicado");

                bool updated = await UpdateCurrentEvent(draft, cancellationToken);
                if (!updated)
                    return Result.Fail("Evento não existe");

                draft.PublishDraft();

                await _db.EventDraftCollection.ReplaceOneAsync(t =>
                    t.Id == draft.Id, draft,
                    cancellationToken: cancellationToken
                );

                await UpdateTrackEvent(draft, cancellationToken);

                return Result.Ok();
            }

            private async Task<bool> UpdateCurrentEvent(EventDraft draft, CancellationToken token)
            {
                var dbEvent = await _db.EventCollection.AsQueryable()
                    .Where(mod => mod.Id == draft.EventId)
                    .FirstOrDefaultAsync();

                if (dbEvent == null)
                    return false;

                string config = _configuration[$"EcommerceIntegration:Active"];

                dbEvent.EcommerceId = draft.EcommerceId;

                if (!draft.EcommerceId.HasValue && draft.CreateInEcommerce && config == "True")
                {
                    var response = await CreateEcommerceProduct(dbEvent);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var parsed = JObject.Parse(content);
                        dbEvent.EcommerceId = (long)parsed["product"]["id"];
                    }
                    else
                    {
                        var error = ErrorLog.Create(
                            ErrorLogTypeEnum.EcommerceIntegration,
                            "create-module", content
                        ).Data;

                        await _db.ErrorLogCollection.InsertOneAsync(
                            error, cancellationToken: token
                        );
                    }
                }

                dbEvent.Title = draft.Title;
                dbEvent.Excerpt = draft.Excerpt;
                dbEvent.ImageUrl = draft.ImageUrl;
                dbEvent.InstructorId = draft.InstructorId;
                dbEvent.Instructor = draft.Instructor;
                dbEvent.InstructorMiniBio = draft.InstructorMiniBio;
                dbEvent.InstructorImageUrl = draft.InstructorImageUrl;
                dbEvent.Tags = draft.Tags;
                dbEvent.VideoUrl = draft.VideoUrl;
                dbEvent.VideoDuration = draft.VideoDuration;
                dbEvent.Duration = draft.Duration;
                dbEvent.PrepQuiz = draft.PrepQuiz;
                dbEvent.PrepQuizQuestionList = draft.PrepQuizQuestionList;
                dbEvent.CertificateUrl = draft.CertificateUrl;
                dbEvent.TutorsIds = draft.TutorsIds;
                dbEvent.StoreUrl = draft.StoreUrl;
                dbEvent.Schedules = draft.Schedules;
                dbEvent.Requirements = draft.Requirements;
                dbEvent.SupportMaterials = draft.SupportMaterials;

                await _db.EventCollection.ReplaceOneAsync(t =>
                    t.Id == dbEvent.Id, dbEvent,
                    cancellationToken: token
                );

                return true;
            }

            private async Task<bool> UpdateTrackEvent(EventDraft draft, CancellationToken token)
            {
                var dbTrack = await _db.TrackCollection.AsQueryable()
                    .Where(x => x.EventsConfiguration.Any(y => y.EventId == draft.EventId))
                    .ToListAsync(token);

                if (dbTrack == null || dbTrack.Count == 0)
                    return false;

                var models = new List<WriteModel<Track>>();
                foreach (Track track in dbTrack)
                {
                    var events = track.EventsConfiguration.Where(y => y.EventId == draft.EventId).ToList();
                    if (events != null && events.Count > 0)
                    {
                        foreach (TrackEvent trackEvent in events)
                        {
                            trackEvent.Title = draft.Title;
                        }
                        var upd = Builders<Track>.Update.Set(x => x.EventsConfiguration, track.EventsConfiguration);
                        models.Add(new UpdateOneModel<Track>(new BsonDocument("_id", track.Id), upd));
                    }
                }
                await _db.TrackCollection.BulkWriteAsync(models);
                return true;
            }

            private async Task<HttpResponseMessage> CreateEcommerceProduct(Event dbEvent)
            {
                var product = new EcommerceItem
                {
                    product = new ProductItem
                    {
                        visible = false,
                        title = dbEvent.Title,
                        type = "simple",
                        regular_price = "0",
                        description = dbEvent.Excerpt
                    }
                };

                return await Woocommerce.CreateProduct(
                    product, _configuration
                );
            }
        }
    }
}
