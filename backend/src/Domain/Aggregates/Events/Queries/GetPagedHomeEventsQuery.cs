using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Locations;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Queries
{
    public class GetPagedHomeEventsQuery
    {
        public class Contract : CommandContract<Result<List<EventItem>>>
        {
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class EventItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt {get;set; }
            public string ImageUrl { get; set; }
            public EventScheduleItem NextSchedule { get; set; }
            public List<EventScheduleItem> Schedules { get; set; }
            public List<RequirementItem> Requirements { get; set; }
            public List<ModuleProgressInfo> ModuleProgressInfo { get; set; }
            public bool Recommended { get; set; } = false;
            public DateTimeOffset DeletedAt { get; set; }

            public EventItem()
            {
                Schedules = new List<EventScheduleItem>();
            }
        }

        public class EventScheduleItem
        {
            public DateTimeOffset EventDate { get; set; }
            public ObjectId Id { get; set; }
            public DateTimeOffset SubscriptionStartDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
            public DateTimeOffset? ForumStartDate { get; set; }
            public DateTimeOffset? ForumEndDate { get; set; }
            public int Duration { get; set; }
            public bool Published { get; set; }
            public Location Location { get; set; }
        }

        public class EventApplicationItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId ScheduleId { get; set; }
        }

        public class RequirementItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public bool? Optional { get; set; }
            public int? Level { get; set; }
            public decimal? Percentage { get; set; }
            public ContractUserProgress RequirementValue { get; set; }
        }

        public class ContractUserProgress
        {
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class ModuleNameItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
        }

        public class ModuleProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
        }

        public class UserTrackItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId TrackId { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<EventItem>>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<List<EventItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var userTrackEvents = new List<EventItem>();
                    var events = new List<EventItem>();

                    var user = await GetUserById(userId, cancellationToken);

                    var eventsDb = await _db.Database
                        .GetCollection<EventItem>("Events")
                        .AsQueryable()
                        .Where(x =>
                            (x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue) &&
                             x.Schedules != null &&
                             x.Schedules.Any(sch =>
                                 sch.EventDate > DateTimeOffset.Now
                             )
                        )
                        .ToListAsync();

                    for (int i = 0; i < eventsDb.Count(); i++)
                    {
                        eventsDb[i].Schedules = eventsDb[i].Schedules.ToList().Where(x =>
                            (user.Location != null && x.Location != null) ? user.Location.Id == x.Location.Id : x.EventDate != null
                        ).ToList();
                    }
                    
                    if (request.UserRole == "Student" || request.UserRole == "Admin")
                    {
                        userTrackEvents = await GetRecommendedEvents(userId, eventsDb, cancellationToken);
                        events = eventsDb.Where(x =>
                            !userTrackEvents.Select(ev => ev.Id).Contains(x.Id)
                        ).ToList();

                        events = events.Concat(userTrackEvents).ToList();
                    }

                    if (request.UserRole == "BusinessManager")
                    {
                        events = await GetBusinessManagerEvents(userId, eventsDb, cancellationToken);
                    }

                    foreach (EventItem evt in events)
                    {
                        evt.Schedules = evt.Schedules.OrderBy(x => x.EventDate).ToList();
                        
                        if (request.UserRole == "Student" && !evt.Recommended)
                            evt.Schedules = evt.Schedules.Where(x => x.Published).ToList();

                        evt.Requirements = evt.Requirements ?? new List<RequirementItem>();
                        foreach (var req in evt.Requirements.ToList())
                        {
                            var query = await _db.Database
                                .GetCollection<ModuleNameItem>("Modules")
                                .FindAsync(x =>
                                    x.Id == req.ModuleId,
                                    cancellationToken: cancellationToken
                                );

                            var mod = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                            if (mod == null)
                            {
                                evt.Requirements.Remove(req);
                                continue;
                            }

                            req.Title = mod.Title;
                            req.Level = req.RequirementValue.Level;
                            req.Percentage = req.RequirementValue.Percentage;
                            req.RequirementValue = null;
                        }

                        evt.ModuleProgressInfo = _db
                            .UserModuleProgressCollection
                            .AsQueryable()
                            .Where(x => x.UserId == userId)
                            .Select(x => new ModuleProgressInfo()
                            {
                                Level = x.Level,
                                ModuleId = x.ModuleId,
                                Progress = x.Progress
                            })
                            .ToList();
                    }

                    return Result.Ok(
                        events.Where(
                            ev => ev.Schedules.Count > 0
                        ).ToList()
                    );
                }
                catch (Exception err)
                {
                    return Result.Fail<List<EventItem>>(
                        $"Ocorreu um erro ao buscar os eventos: {err.Message}"
                    );
                }
            }

            private async Task<List<EventItem>> GetRecommendedEvents(ObjectId userId, List<EventItem> dbEvents, CancellationToken token)
            {
                var user = await GetUserById(userId, token);

                if(user.TracksInfo == null)
                {
                    return new List<EventItem>();
                }
                var userTrackIds = user.TracksInfo.Where(x => x.Blocked != true).Select(x => x.Id).ToList();

                var trackEventsConfigurations = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .AsQueryable()
                    .Where(x => userTrackIds.Contains(x.Id))
                    .Select(x => x.EventsConfiguration)
                    .ToListAsync();

                var trackEventIds = new List<ObjectId>();
                var recommendedIds = new List<ObjectId>();
                var schedulesIds = new List<ObjectId>();

                foreach (List<TrackEventItem> eventConfig in trackEventsConfigurations)
                    trackEventIds.AddRange(eventConfig.Select(x => x.EventId));

                if (user.EventsInfo != null)
                {
                    schedulesIds = user.EventsInfo.Where(x => x.Blocked != true).Select(ev => ev.Id).ToList();

                    recommendedIds = dbEvents.Where(dbEv =>
                        dbEv.Schedules.Any(sch => schedulesIds.Contains(sch.Id))
                    ).Select(
                        ev => ev.Id
                    ).ToList();
                }

                var events = dbEvents
                    .Where(x =>
                        trackEventIds.Contains(x.Id) ||
                        recommendedIds.Contains(x.Id)
                    )
                    .ToList();

                foreach (EventItem evt in events)
                {
                    if (!trackEventIds.Contains(evt.Id))
                    {
                        evt.Schedules = evt.Schedules.Where(
                            sch => schedulesIds.Contains(sch.Id)
                        ).ToList();
                    }

                    evt.Recommended = true;
                }

                return events;
            }

            private async Task<List<EventItem>> GetBusinessManagerEvents(ObjectId userId, List<EventItem> dbEvents, CancellationToken token)
            {
                var user = await GetUserById(userId, token);

                if (user.EventsInfo == null)
                {
                    return new List<EventItem>();
                }
                var userEventsIds = user.EventsInfo.Select(x => x.Id).ToList();

                var events = dbEvents
                    .Where(x =>
                        userEventsIds.Contains(x.Id)
                    )
                    .ToList();

                return events;
            }

            private async Task<User> GetUserById(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(
                        x => x.Id == userId,
                        cancellationToken: token
                     );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }
        }
    }
}
