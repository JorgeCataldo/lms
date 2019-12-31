using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Responsibles;
using Domain.Data;
using Domain.Base;
using Domain.Extensions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTracksEffortPerformancesPagedQuery
    {
        public class Contract : CommandContract<Result<PagedTrackItems>>
        {
            public string UserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
        }

        public class PagedTrackItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<TrackItem> Tracks { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public bool Recommended { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
            public ObjectId? DeletedBy { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public int EventCount { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public int ModuleCount { get; set; }
            public decimal Duration { get; set; }
            public bool? Published { get; set; }
            public bool? Blocked { get; set; }
            public bool Subordinate { get; set; }
            public bool Instructor { get; set; }
        }
        
        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedTrackItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedTrackItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var options = new FindOptions<TrackItem>()
                {
                    Sort = Builders<TrackItem>.Sort.Ascending("Title"),
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<TrackItem> filters = SetFilters(request);
                var collection = _db.Database.GetCollection<TrackItem>("Tracks");

                var qry = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var tracksList = await qry.ToListAsync(cancellationToken);

                var result = new PagedTrackItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(
                        filters, cancellationToken: cancellationToken
                    ),
                    Tracks = tracksList.Select(x => {
                        x.EventCount = x.EventsConfiguration.Count;
                        x.ModuleCount = x.ModulesConfiguration.Count;
                        return x;
                    }).ToList()
                };
                
                return Result.Ok(result);
            }

            private FilterDefinition<TrackItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<TrackItem>.Empty;
                var builder = Builders<TrackItem>.Filter;

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                {
                    filters = filters & builder.Regex("title", new BsonRegularExpression("/" + request.Filters.Term + "/is"));
                }               

                return filters;
            }
        }
    }
}
