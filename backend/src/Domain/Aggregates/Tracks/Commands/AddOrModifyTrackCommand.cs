using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Aggregates.Modules;
using Domain.Data;
using Domain.ECommerceIntegration.Woocommerce;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class AddOrModifyTrackCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public List<string> Tags { get; set; }
            public bool Published { get; set; }
            public string CertificateUrl { get; set; }
            public string UserRole { get; set; }
            public bool RequireUserCareer { get; set; }
            public int? AllowedPercentageWithoutCareerInfo { get; set; }

            public List<ContractTrackEvent> EventsConfiguration { get; set; }
            public List<ContractTrackModule> ModulesConfiguration { get; set; }

            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public bool? MandatoryCourseVideo { get; set; }
            public string CourseVideoUrl { get; set; }
            public int? CourseVideoDuration { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
            public bool? CreateInEcommerce { get; set; } = false;
            public string ProfileTestId { get; set; }
            public string ProfileTestName { get; set; } = null;
            public int? ValidFor { get; set; }

            public Contract()
            {
                ModulesConfiguration = new List<ContractTrackModule>();
                Tags = new List<string>();
            }
        }

        public class ContractTrackModule
        {
            public string ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int Order { get; set; }
            public decimal Weight { get; set; }
            public DateTimeOffset? CutOffDate { get; set; }
            public decimal BDQWeight { get; set; }
            public decimal EvaluationWeight { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }
        
        public class ContractTrackEvent
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public decimal Weight { get; set; }
            public DateTimeOffset? CutOffDate { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
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
                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail<Contract>("Acesso Negado");
                                
                var mConfigResult = request.ModulesConfiguration.Select(x => 
                    TrackModule.Create(
                        ObjectId.Parse(x.ModuleId), x.Title,
                        x.Level, x.Percentage, x.Order,
                        x.Weight, x.CutOffDate, x.BDQWeight, x.EvaluationWeight,
                        x.AlwaysAvailable, x.OpenDate, x.ValuationDate
                    )
                ).ToArray();

                var mCombinedResult = Result.Combine(mConfigResult);
                if (mCombinedResult.IsFailure)
                    return Result.Fail<Contract>(mCombinedResult.Error);

                if (request.EventsConfiguration == null)
                    request.EventsConfiguration = new List<ContractTrackEvent>();

                var eConfigResult = request.EventsConfiguration.Select(x =>
                    TrackEvent.Create(
                        ObjectId.Parse(x.EventId),
                        ObjectId.Parse(x.EventScheduleId),
                        x.Order, x.Title, x.Weight, x.CutOffDate, x.AlwaysAvailable,
                        x.OpenDate, x.ValuationDate
                    )
                ).ToArray();

                var eCombinedResult = Result.Combine(eConfigResult);
                if (eCombinedResult.IsFailure)
                    return Result.Fail<Contract>(eCombinedResult.Error);

                var tags = new List<ValueObjects.Tag>();
                foreach (var tagStr in request.Tags)
                {
                    var tag = ValueObjects.Tag.Create(tagStr);
                    tags.Add(tag.Data);
                }

                var trackDuration = await GetTrackDuration(request, cancellationToken);

                ObjectId? profileTestId = null;
                if (!String.IsNullOrEmpty(request.ProfileTestId))
                    profileTestId = ObjectId.Parse(request.ProfileTestId);

                var newObject = Track.Create(
                    request.Title, request.Description, request.ImageUrl, 
                    eConfigResult.Select(x => x.Data).ToList(),
                    mConfigResult.Select(x=> x.Data).ToList(),
                    0, trackDuration, tags, request.Published,
                    request.CertificateUrl, request.StoreUrl, request.EcommerceUrl,
                    profileTestId, request.ProfileTestName, request.ValidFor
                 );

                if (newObject.IsFailure)
                    return Result.Fail<Contract>(newObject.Error);

                var newTrack = newObject.Data;

                if (string.IsNullOrEmpty(request.Id))
                {
                    string config = _configuration[$"EcommerceIntegration:Active"];

                    if (request.CreateInEcommerce.HasValue && request.CreateInEcommerce.Value && config == "True")
                    {
                        var response = await CreateEcommerceProduct(newTrack);
                        string content = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            var parsed = JObject.Parse(content);
                            newTrack.EcommerceProducts = new List<EcommerceProduct>();
                            newTrack.EcommerceProducts.Add(
                                EcommerceProduct.Create(
                                    (long)parsed["product"]["id"], 1, false, null, false, null, null, null, null
                                ).Data
                            );
                        }
                        else
                        {
                            var error = ErrorLog.Create(
                                ErrorLogTypeEnum.EcommerceIntegration,
                                "create-track", content
                            ).Data;

                            await _db.ErrorLogCollection.InsertOneAsync(
                                error, cancellationToken: cancellationToken
                            );
                        }
                    }

                    newTrack.RequireUserCareer = request.RequireUserCareer;
                    newTrack.AllowedPercentageWithoutCareerInfo = request.AllowedPercentageWithoutCareerInfo;

                    await _db.TrackCollection.InsertOneAsync(
                        newTrack,
                        cancellationToken: cancellationToken
                    );
                    request.Id = newTrack.Id.ToString();
                }
                else
                {
                    var query = await _db.Database
                        .GetCollection<Track>("Tracks")
                        .FindAsync(x =>
                            x.Id == ObjectId.Parse(request.Id),
                            cancellationToken: cancellationToken
                        );

                    var track = await query.FirstOrDefaultAsync(
                        cancellationToken: cancellationToken
                    );
                    
                    if (track == null)
                        return Result.Fail<Contract>("Trilha não Encontrada");

                    track.Title = newObject.Data.Title;
                    track.Description = newObject.Data.Description;
                    track.EventsConfiguration = newObject.Data.EventsConfiguration;
                    track.ImageUrl= newObject.Data.ImageUrl;
                    track.ModulesConfiguration = newObject.Data.ModulesConfiguration;
                    track.UpdatedAt = DateTimeOffset.UtcNow;
                    track.Tags = newObject.Data.Tags;
                    track.Duration = newObject.Data.Duration;
                    track.Published = newObject.Data.Published;
                    track.CertificateUrl = newObject.Data.CertificateUrl;
                    track.VideoUrl = request.VideoUrl;
                    track.VideoDuration = request.VideoDuration;
                    track.MandatoryCourseVideo = request.MandatoryCourseVideo.HasValue ?
                        request.MandatoryCourseVideo.Value : false;
                    track.CourseVideoUrl = request.CourseVideoUrl;
                    track.CourseVideoDuration = request.CourseVideoDuration;
                    track.StoreUrl = request.StoreUrl;
                    track.EcommerceUrl = request.EcommerceUrl;
                    track.RequireUserCareer = request.RequireUserCareer;
                    track.AllowedPercentageWithoutCareerInfo = request.AllowedPercentageWithoutCareerInfo;
                    track.ProfileTestId = profileTestId;
                    track.ProfileTestName = request.ProfileTestName;
                    track.ValidFor = request.ValidFor;

                    await _db.TrackCollection.ReplaceOneAsync(t =>
                        t.Id == track.Id, track,
                        cancellationToken: cancellationToken
                    );

                    if (track.EcommerceProducts != null)
                    {
                        string config = _configuration[$"EcommerceIntegration:Active"];
                        if (config == "True")
                        {
                            foreach (var product in track.EcommerceProducts)
                                await UpdateEcommerceProduct(track, product.EcommerceId);
                        }
                    }
                }

                return Result.Ok(request);
            }

            private async Task<HttpResponseMessage> CreateEcommerceProduct(Track track)
            {
                var product = new EcommerceItem
                {
                    product = new ProductItem
                    {
                        visible = false,
                        title = track.Title,
                        type = "simple",
                        regular_price = "0",
                        description = track.Description
                    }
                };

                return await Woocommerce.CreateProduct(
                    product, _configuration
                );
            }

            private async Task<decimal> GetTrackDuration(Contract request, CancellationToken token)
            {
                decimal totalDuration = 0;

                foreach (var config in request.ModulesConfiguration)
                {
                    var moduleId = ObjectId.Parse(config.ModuleId);

                    var query = await _db.Database
                        .GetCollection<Module>("Modules")
                        .FindAsync(x =>
                            x.Id == moduleId,
                            cancellationToken: token
                        );

                    var dbModule = await query.FirstAsync(
                        cancellationToken: token
                    );

                    totalDuration = totalDuration +
                        dbModule.VideoDuration.Minutes +
                        dbModule.Subjects.Sum(subj =>
                            subj.Contents.Sum(cont =>
                                cont.Duration
                            )
                        );
                }

                return totalDuration;
            }

            private async Task<HttpResponseMessage> UpdateEcommerceProduct(Track track, long ecommerceId)
            {
                var product = new EcommerceItem {
                    product = new ProductItem {
                        id = ecommerceId,
                        title = track.Title
                    }
                };

                return await Woocommerce.UpdateProduct(
                    product, _configuration
                );
            }
        }
    }
}
