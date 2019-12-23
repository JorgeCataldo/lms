using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserProfileQuery
    {
        
        public class Contract : CommandContract<Result<UserItem>>
        {
            public string Id { get; set; }
            public string CurrentUserId { get; set; }
            public string UserRole { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string UserName { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public bool IsBlocked { get; set; }
            public bool IsEmailConfirmed { get; set; }
            public string ImageUrl { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string RegistrationId { get; set; }
            public string Info { get; set; }
            public List<UserProgressItem> ModulesInfo { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<UserProgressItem> EventsInfo { get; set; }
            public List<AcquiredKnowledge> AcquiredKnowledge { get; set; }
            public string LineManager { get; set; }
            public string LineManagerEmail { get; set; }
            public string Gender { get; set; }
            public bool? Manager { get; set; }
            public long? Cge { get; set; }
            public long? IdCr { get; set; }
            public string CoHead { get; set; }
            public List<CalendarEventItem> CalendarEvents { get; set; }
            public Address Address { get; set; }
            public DocumentType? Document { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentEmitter { get; set; }
            public DateTimeOffset? EmitDate { get; set; }
            public DateTimeOffset? ExpirationDate { get; set; }
            public bool? SpecialNeeds { get; set; }
            public string SpecialNeedsDescription { get; set; }
            public string LinkedIn { get; set; }
        }

        public class AcquiredKnowledge
        {
            public int Level { get; set; }
            public string Name { get; set; }

            public AcquiredKnowledge(int level, string name)
            {
                Level = level;
                Name = name;
            }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
            public ObjectId? ScheduleId { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public List<ValueObjects.Tag> Tags { get; set; }
            public bool CanBlock { get; set; } = false;
            public bool Blocked { get; set; } = false;
            public int ClassRank { get; set; }
        }

        public class ProgressItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public DateTimeOffset DeletedAt { get; set; }
        }

        public class UserEventItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId ScheduleId { get; set; }
        }

        public class CalendarEventItem
        {
            public ObjectId? EventId { get; set; }
            public ObjectId? EventScheduleId { get; set; }
            public int? Duration { get; set; }
            public string Title { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public string TrackId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<UserItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<UserItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                /*if (request.UserRole == "Student" && request.CurrentUserId != request.Id)
                    return Result.Fail<UserItem>("Acesso Negado");*/

                var user = await GetUser(request, cancellationToken);

                user.TracksInfo = user.TracksInfo ?? new List<UserProgressItem>();
                user.ModulesInfo = user.ModulesInfo ?? new List<UserProgressItem>();
                user.EventsInfo = user.EventsInfo ?? new List<UserProgressItem>();

                user.CalendarEvents = new List<CalendarEventItem>();
                    
                var showBlocked = request.UserRole != "Student";
                if (!showBlocked)
                    user.TracksInfo = user.TracksInfo.Where(x => !x.Blocked).ToList();

                user = await FillUserTracks(user);

                var tracksIdsToRemove = user.TracksInfo.Where(x => x.Blocked == true).Select(x => x.Id).ToList();
                var tracksToRemoveQuery = _db.TrackCollection.AsQueryable().Where(x =>
                    (x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue) &&
                    tracksIdsToRemove.Contains(x.Id)
                );

                var modulesQry = _db.ModuleCollection.AsQueryable();

                var joinRecomendedModuleQry = from p in user.ModulesInfo.AsQueryable()
                    join m in modulesQry on p.Id equals m.Id
                    where m.DeletedAt == null || m.DeletedAt == DateTimeOffset.MinValue
                    select new UserProgressItem()
                    {
                        Id = p.Id,
                        Level = p.Level,
                        Percentage = p.Percentage,
                        Title = m.Title,
                        ImageUrl = m.ImageUrl,
                        Tags = m.Tags,
                        CanBlock = true,
                        Blocked = p.Blocked
                    };
                var modulesToRemove = tracksToRemoveQuery.Select(x => x.ModulesConfiguration).ToList();
                var modulesIdsToRemove = new List<ObjectId>();
                modulesToRemove.ForEach(modConfig =>
                {
                    modulesIdsToRemove.AddRange(modConfig.Select(x => x.ModuleId));
                });
                var moduleProgressQry = _db.UserModuleProgressCollection.AsQueryable().Where(
                    x => x.UserId == user.Id && 
                    !modulesIdsToRemove.Contains(x.ModuleId)
                );
                var joinQry = from p in moduleProgressQry
                    join m in modulesQry on p.ModuleId equals m.Id
                    where m.DeletedAt == null || m.DeletedAt == DateTimeOffset.MinValue
                    select new UserProgressItem()
                    {
                        Id = p.ModuleId,
                        Level = p.Level,
                        Percentage = p.Progress,
                        Title = m.Title,
                        ImageUrl = m.ImageUrl,
                        Tags = m.Tags
                    };

                user.ModulesInfo = showBlocked ? joinQry.ToList().Concat(joinRecomendedModuleQry.ToList()).ToList() : 
                    joinQry.ToList().Concat(joinRecomendedModuleQry.Where(x => x.Blocked != true).ToList()).ToList();

                user.AcquiredKnowledge = user.ModulesInfo
                    .Where(x => x.Level > 0)
                    .SelectMany(x => x.Tags.Select(t => new AcquiredKnowledge(x.Level - 1, t.Name)))
                    .ToList();

                user.EventsInfo = showBlocked ? user.EventsInfo : user.EventsInfo.Where(x => x.Blocked != true).ToList();
                foreach (var req in user.EventsInfo.Reverse<UserProgressItem>())
                {
                    var eveQuery = await _db.Database
                        .GetCollection<Event>("Events")
                        .FindAsync(x =>
                            (x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue) &&
                            x.Schedules.Any(sch => sch.Id == req.Id),
                            cancellationToken: cancellationToken
                        );
                    var eve = await eveQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (eve == null)
                        user.EventsInfo.Remove(req);
                    else
                    {
                        req.ScheduleId = req.Id;
                        req.Id = eve.Id;
                        req.Title = eve.Title;
                        req.ImageUrl = eve.ImageUrl;
                        req.CanBlock = true;

                        var isInCalendar = user.CalendarEvents.Any(calEv => calEv.EventScheduleId == req.ScheduleId);
                        if (!isInCalendar && eve.Schedules != null && req.ScheduleId.HasValue)
                        {
                            var schedule = eve.Schedules.FirstOrDefault(sch => sch.Id == req.ScheduleId.Value);
                            if (schedule != null)
                            {
                                user.CalendarEvents.Add(new CalendarEventItem {
                                    Duration = schedule.Duration,
                                    EventDate = schedule.EventDate,
                                    EventId = eve.Id,
                                    EventScheduleId = schedule.Id,
                                    Title = "Evento: " + eve.Title
                                });
                            }
                        }
                    }
                }

                var eventsToRemove = tracksToRemoveQuery.Select(x => x.EventsConfiguration).ToList();
                var eventsSchedulesIdsToRemove = new List<ObjectId>();
                eventsToRemove.ForEach(eveConfig => {
                    eventsSchedulesIdsToRemove.AddRange(
                        eveConfig.Select(x => x.EventScheduleId)
                    );
                });

                user = await GetApplications(user, eventsSchedulesIdsToRemove, cancellationToken);

                return Result.Ok(user);
            }

            private async Task<UserItem> GetUser(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.Id);

                var qry = await _db.Database
                    .GetCollection<UserItem>("Users")
                    .FindAsync(x => x.Id == userId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<UserItem> FillUserTracks(UserItem user)
            {
                var tracksIds = user.TracksInfo.Select(x => x.Id).ToList();
                var tracksQuery = _db.TrackCollection.AsQueryable().Where(x =>
                    tracksIds.Contains(x.Id)
                );
                var tracks = await ((IAsyncCursorSource<Track>)tracksQuery).ToListAsync();

                user.TracksInfo = user.TracksInfo.Where(t =>
                    tracks.Any(tQ => tQ.Id == t.Id)
                ).ToList();

                foreach (var userTrack in user.TracksInfo)
                {
                    Track track = tracks.Where(x => x.Id == userTrack.Id).First();
                    userTrack.Title = track.Title;
                    userTrack.ImageUrl = track.ImageUrl;
                    userTrack.CanBlock = true;

                    var trackEventsIds = track.EventsConfiguration.Select(e => e.EventId);
                    var dbEventsQuery = _db.EventCollection.AsQueryable()
                        .Where(ev => trackEventsIds.Contains(ev.Id));
                    var dbEvents = await ((IAsyncCursorSource<Event>)dbEventsQuery).ToListAsync();

                    if (!track.Published)
                        userTrack.ClassRank = await GetUserTrackRanking(track, user.Id);

                    foreach (var trackEvent in track.EventsConfiguration)
                    {
                        var dbEvent = dbEvents.FirstOrDefault(ev => ev.Id == trackEvent.EventId);
                        if (dbEvent != null)
                        {
                            var schedule = dbEvent.Schedules.FirstOrDefault(sch => sch.Id == trackEvent.EventScheduleId);

                            if (schedule != null)
                            {
                                user.CalendarEvents.Add(new CalendarEventItem
                                {
                                    Duration = schedule.Duration,
                                    EventDate = schedule.EventDate,
                                    EventId = trackEvent.EventId,
                                    EventScheduleId = trackEvent.EventScheduleId,
                                    Title = track.Title + ": " + trackEvent.Title,
                                    TrackId = track.Id.ToString()
                                });
                            }
                        }
                    }

                    if (track.CalendarEvents != null)
                    {
                        foreach (var calEvent in track.CalendarEvents)
                        {
                            user.CalendarEvents.Add(new CalendarEventItem
                            {
                                Duration = calEvent.Duration,
                                EventDate = calEvent.EventDate,
                                Title = track.Title + ": " + calEvent.Title,
                                TrackId = track.Id.ToString()
                            });
                        }
                    }
                }

                return user;
            }

            private async Task<UserItem> GetApplications(
                UserItem user, List<ObjectId> eventsSchedulesIdsToRemove, CancellationToken token
            ) {
                var query = await _db.Database
                    .GetCollection<UserEventItem>("EventApplications")
                    .FindAsync(x =>
                        x.UserId == user.Id &&
                        !eventsSchedulesIdsToRemove.Contains(x.ScheduleId),
                        cancellationToken: token
                    );

                var eveApplications = await query.ToListAsync();
                var eventsIds = eveApplications.Select(e => e.EventId);

                var dbEventsQuery = await _db.Database
                    .GetCollection<ProgressItem>("Events")
                    .FindAsync(x => (x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue) && eventsIds.Contains(x.Id), cancellationToken: token);

                var dbEvents = await dbEventsQuery.ToListAsync(cancellationToken: token);

                foreach (var eveapp in eveApplications)
                {
                    var dbEvent = dbEvents.Where(x => x.Id == eveapp.EventId).FirstOrDefault();

                    if (dbEvent != null)
                    {
                        user.EventsInfo.Add(new UserProgressItem
                        {
                            Id = eveapp.EventId,
                            ScheduleId = eveapp.ScheduleId,
                            Title = dbEvent.Title,
                            ImageUrl = dbEvent.ImageUrl
                        });
                    }
                }

                return user;
            }

            private async Task<int> GetUserTrackRanking(Track track, ObjectId userId)
            {
                var query = _db.UserCollection.AsQueryable().Where(x =>
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                ).Select(u => u.Id);

                var studentsIds = await ((IAsyncCursorSource<ObjectId>)query).ToListAsync();

                var trackModules = track.ModulesConfiguration.Select(m => m.ModuleId).ToList();

                var progressQuery = _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        trackModules.Contains(x.ModuleId) &&
                        studentsIds.Contains(x.UserId)
                    );

                var moduleUserProgressList = await ((IAsyncCursorSource<UserModuleProgress>)progressQuery).ToListAsync();

                var moduleUserProgress = moduleUserProgressList
                    .GroupBy(m => m.UserId)
                    .Select(g => g.Aggregate((acc, x) => {
                        acc.Points = acc.Points + x.Points;
                        return acc;
                    }))
                    .OrderByDescending(m => m.Points)
                    .ToList();

                int index = moduleUserProgress.FindIndex(m => m.UserId == userId);

                return index >= 0 ? index + 1 : 0;
            }
        }
    }
}
