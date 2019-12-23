using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.Modules;
using Microsoft.Extensions.Configuration;
using Domain.Aggregates.Events;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackOverviewGradesQuery
    {
        public class Contract : CommandContract<Result<List<GradeItem>>>
        {
            public string TrackId { get; set; }
            public string UserRole { get; set; }
        }

        public class GradeItem
        {
            public string Name { get; set; }
            public List<TrackModuleGradeItem> ModuleGrade { get; set; }
            public List<TrackEventGradeItem> EventGrade { get; set; }
            public List<TrackEventPresenceItem> EventPresence { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class RelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class ModuleProgressItem
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId UserId { get; set; }
            public int Points { get; set; }
        }

        public class EventApplicationItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId UserId { get; set; }
            public decimal? OrganicGrade { get; set; }
            public decimal? InorganicGrade { get; set; }
            public bool? UserPresence { get; set; }
            public List<CustomEventGradeValue> CustomEventGradeValues { get; set; }
        }

        public class TrackModuleGradeItem
        {
            public string ModuleName { get; set; }
            public decimal Grade { get; set; }
            public decimal WeightGrade { get; set; }
        }

        public class TrackEventGradeItem
        {
            public string EventName { get; set; }
            public decimal? FinalGrade { get; set; }
            public decimal? WeightGrade { get; set; }
        }

        public class TrackItemWeight
        {
            public ObjectId ItemId { get; set; }
            public decimal Weight { get; set; }
        }

        public class TrackEventPresenceItem
        {
            public string EventName { get; set; }
            public bool? UserPresence { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<GradeItem>>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<List<GradeItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                string appEventGradeKeys = _configuration[$"Permissions:EventGradeKeys"];
                var eventGradeKeys = new List<string>();
                var hasCustomEventGrades = false;
                if (!string.IsNullOrEmpty(appEventGradeKeys))
                {
                    eventGradeKeys = appEventGradeKeys.Split(',').ToList();
                    hasCustomEventGrades = true;
                }

                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" &&
                    request.UserRole != "Secretary" && request.UserRole != "Recruiter")
                    return Result.Fail<List<GradeItem>>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<List<GradeItem>>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);

                var dbUsers = await _db.UserCollection
                    .AsQueryable()
                    .Where(
                        x => x.TracksInfo != null &&
                        x.TracksInfo.Any(y => y.Id == trackId)
                    )
                    .Select(x => new UserItem
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var userIds = dbUsers.Select(x => x.Id).ToList();

                var dbTrack = await _db.TrackCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == trackId);

                if (dbTrack == null)
                    return Result.Fail<List<GradeItem>>("Trilha não existe");

                var trackItemsWeight = new List<TrackItemWeight>();

                trackItemsWeight.AddRange(dbTrack.EventsConfiguration
                    .Select(x => new TrackItemWeight
                    {
                        ItemId = x.EventId,
                        Weight = x.Weight
                    }).ToList());

                trackItemsWeight.AddRange(dbTrack.ModulesConfiguration
                    .Select(x => new TrackItemWeight
                    {
                        ItemId = x.ModuleId,
                        Weight = x.Weight
                    }).ToList());

                if (!trackItemsWeight.Any(x => x.Weight > 0))
                {
                    foreach (TrackItemWeight item in trackItemsWeight)
                    {
                        item.Weight = 100 / (decimal)trackItemsWeight.Count;
                    }
                }

                var dbEvents = dbTrack.EventsConfiguration
                    .Select(x => new RelationalItem
                    {
                        Id = x.EventId,
                        Name = x.Title
                    }).ToList();

                var eventsIds = dbEvents.Select(x => x.Id);

                var modulesIds = dbTrack.ModulesConfiguration.Select(x => x.ModuleId).ToList();

                var modulesGrades = Module.GetModulesGrades(_db, userIds, modulesIds).Data;

                var dbEventApplication = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => userIds.Contains(x.UserId) && eventsIds.Contains(x.EventId))
                    .Select(x => new EventApplicationItem
                    {
                        EventId = x.EventId,
                        UserId = x.UserId,
                        OrganicGrade = x.OrganicGrade,
                        InorganicGrade = x.InorganicGrade,
                        UserPresence = x.UserPresence,
                        CustomEventGradeValues = x.CustomEventGradeValues
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var usersGrade =
                    from userId in userIds
                    join user in dbUsers on userId equals user.Id
                    select new GradeItem
                    {
                        Name = user.Name,
                        ModuleGrade = GetUserModuleGrade(user.Id, modulesGrades, trackItemsWeight),
                        EventGrade = GetUserEventGrade(user.Id, dbEventApplication, dbEvents, hasCustomEventGrades, eventGradeKeys, trackItemsWeight),
                        EventPresence = GetUserEventPresence(user.Id, dbEventApplication, dbEvents)
                    };

                return Result.Ok(usersGrade.ToList());
            }
            private List<TrackModuleGradeItem> GetUserModuleGrade(ObjectId userId, List<ModuleGradeItem> mods, List<TrackItemWeight> trackItemWeights)
            {
                return mods
                    .Where(x => x.UserGrades.Any(y => y.UserId == userId))
                    .Select(x => new TrackModuleGradeItem
                    {
                        ModuleName = x.ModuleName,
                        Grade = x.UserGrades.First(y => y.UserId == userId).Grade * 10,
                        WeightGrade = ApplyWeight(x.ModuleId, trackItemWeights, x.UserGrades.First(y => y.UserId == userId).Grade * 10) ?? 0
                    }).ToList();
            }

            private List<TrackEventGradeItem> GetUserEventGrade(ObjectId userId, List<EventApplicationItem> eveApps, List<RelationalItem> eves, 
                bool customEventGrades, List<string> keys, List<TrackItemWeight> trackItemWeights)
            {
                return (from eveApp in eveApps
                        join eve in eves on eveApp.EventId equals eve.Id
                        where eveApp.UserId == userId
                        select new TrackEventGradeItem
                        {
                            EventName = eve.Name,
                            FinalGrade = customEventGrades ? GetCustomFinalGrade(eveApp, keys) :
                                GetFinalGrade(eveApp.OrganicGrade, eveApp.InorganicGrade),
                            WeightGrade = ApplyWeight(eve.Id, trackItemWeights, customEventGrades ? GetCustomFinalGrade(eveApp, keys) :
                                GetFinalGrade(eveApp.OrganicGrade, eveApp.InorganicGrade))
                        }).ToList();
            }

            private List<TrackEventPresenceItem> GetUserEventPresence(ObjectId userId, List<EventApplicationItem> eveApps, List<RelationalItem> eves)
            {
                return (from eveApp in eveApps
                        join eve in eves on eveApp.EventId equals eve.Id
                        where eveApp.UserId == userId
                        select new TrackEventPresenceItem
                        {
                            EventName = "Presença - " + eve.Name,
                            UserPresence = eveApp.UserPresence
                        }).ToList();
            }

            private decimal? GetFinalGrade(decimal? OrganicGrade, decimal? InorganicGrade)
            {
                if (InorganicGrade.HasValue && OrganicGrade.HasValue)
                {
                    var sum = InorganicGrade.Value + OrganicGrade.Value;
                    return Math.Round(sum / 2, 2);
                }
                return null;
            }

            private decimal? GetCustomFinalGrade(EventApplicationItem eveapp, List<string> keys)
            {
                if (eveapp.CustomEventGradeValues != null)
                {
                    var customGrades = eveapp.CustomEventGradeValues.Where(x => keys.Contains(x.Key)).ToList();
                    decimal sum = 0;
                    foreach(CustomEventGradeValue grade in customGrades)
                    {
                        sum += grade.Grade ?? 0;
                    }
                    sum = sum / keys.Count();
                }
                return null;
            }

            private decimal? ApplyWeight(ObjectId itemId, List<TrackItemWeight> trackItemWeights, decimal? grade)
            {
                var weightItem = trackItemWeights.FirstOrDefault(x => x.ItemId == itemId);
                if (grade.HasValue  && weightItem != null && weightItem.Weight > 0)
                {
                    return (grade.Value / 10) * weightItem.Weight;
                }
                return null;
            }
        }
    }
}
