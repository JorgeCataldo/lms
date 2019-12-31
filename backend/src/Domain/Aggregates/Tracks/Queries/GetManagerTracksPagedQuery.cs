using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Responsibles;
using Domain.Base;
using Domain.Data;
using Domain.Extensions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetManagerTracksPagedQuery
    {
        public class Contract : CommandContract<Result<PagedTrackItems>>
        {
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
            public bool? Published { get; set; }
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
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
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
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                {
                    return Result.Ok(new PagedTrackItems()
                    {
                        Page = 1,
                        ItemsCount = 0,
                        Tracks = new List<TrackItem>()
                    });
                }

                var options = new FindOptions<TrackItem>()
                {
                    Sort = Builders<TrackItem>.Sort.Ascending("Title"),
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                var userId = ObjectId.Parse(request.UserId);
                var subordinateRecommendedTracks = new List<UserProgress>();
                var InstructorTracks = new List<ObjectId>();
                var allTracks = new List<ObjectId>();
                var recommendedTracks = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => x.Id == userId)
                    .SelectMany(x => x.TracksInfo)
                    .ToListAsync(cancellationToken);

                recommendedTracks = recommendedTracks.Where(x => x.Blocked != true).ToList();
                allTracks.AddRange(recommendedTracks.Select(x => x.Id));

                if (request.UserRole == "Student")
                {
                    var responsible = await _db
                        .Database
                        .GetCollection<Responsible>("Responsibles")
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId);

                    if (responsible != null)
                    {
                        subordinateRecommendedTracks = _db.UserCollection
                            .AsQueryable()
                            .Where(x => responsible.SubordinatesUsersIds.Contains(x.Id))
                            .SelectMany(x => x.TracksInfo)
                            .ToList();

                        subordinateRecommendedTracks = subordinateRecommendedTracks
                            .Where(x => x.Blocked != true)
                            .ToList()
                            .DistinctBy(x => x.Id)
                            .ToList();

                        allTracks.AddRange(subordinateRecommendedTracks.Select(x => x.Id));
                    }

                    var InstructorEventsIds = await _db.EventCollection
                        .AsQueryable()
                        .Where(x => (x.InstructorId == userId || x.TutorsIds.Contains(userId)))
                        .Select(x => x.Id)
                        .ToListAsync(cancellationToken);

                    var InstructorModulesIds = await _db.ModuleCollection
                        .AsQueryable()
                        .Where(x => (x.InstructorId == userId || x.ExtraInstructorIds.Contains(userId) || x.TutorsIds.Contains(userId)))
                        .Select(x => x.Id)
                        .ToListAsync(cancellationToken);

                    if (InstructorEventsIds.Count > 0 || InstructorModulesIds.Count > 0)
                    {
                        var InstructorTracksIds = await _db.TrackCollection
                            .AsQueryable()
                            .Where(x => 
                                x.ModulesConfiguration.Any(y => InstructorModulesIds.Contains(y.ModuleId)) ||
                                x.EventsConfiguration.Any(y => InstructorEventsIds.Contains(y.EventId))
                            )
                            .Select(x => x.Id)
                            .ToListAsync(cancellationToken);

                        InstructorTracks.AddRange(InstructorTracksIds);
                        allTracks.AddRange(InstructorTracksIds);
                    }
                }

                allTracks = allTracks.Distinct().ToList();
                FilterDefinition<TrackItem> filters = SetFilters(request, allTracks);
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
                        x.EventsConfiguration = null;
                        x.ModulesConfiguration = null;
                        return x;
                    }).ToList()
                };

                foreach (var recommendedTrack in recommendedTracks)
                {
                    var track = result.Tracks.FirstOrDefault(x => x.Id == recommendedTrack.Id);
                    if (track != null)
                        track.Recommended = true;
                }

                foreach (var subordinateRecommendedTrack in subordinateRecommendedTracks)
                {
                    var track = result.Tracks.FirstOrDefault(x => x.Id == subordinateRecommendedTrack.Id);
                    if (track != null)
                        track.Subordinate = true;
                }

                foreach (var InstructorTrackId in InstructorTracks)
                {
                    var track = result.Tracks.FirstOrDefault(x => x.Id == InstructorTrackId);
                    if (track != null)
                        track.Instructor = true;
                }

                return Result.Ok(result);
            }

            private FilterDefinition<TrackItem> SetFilters(Contract request, List<ObjectId> specialTrackIds)
            {
                var filters = FilterDefinition<TrackItem>.Empty;
                var builder = Builders<TrackItem>.Filter;

                filters = filters & (
                    builder.Eq(x => x.DeletedAt, null) | 
                    builder.Eq(x => x.DeletedAt, DateTimeOffset.MinValue)
                );

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                    filters = filters & builder.Regex("title",
                                  new BsonRegularExpression("/" + request.Filters.Term + "/is"));

                if (request.UserRole == "Student")
                {
                    filters = filters & (
                        builder.Eq(x => x.Blocked, false) |
                        builder.Eq(x => x.Blocked, null)
                    );
                    if (request.Filters.Published.HasValue)
                    {
                        if (request.Filters.Published.Value)
                        {
                            filters = filters & (
                                builder.Eq(x => x.Published, true) |
                                builder.In(x => x.Id, specialTrackIds)
                            );
                        }
                        else
                        {
                            filters = filters & (
                                builder.Eq(x => x.Published, false) |
                                builder.Eq(x => x.Published, null)
                            );
                        }
                    }
                }

                return filters;
            }
        }
    }
}
