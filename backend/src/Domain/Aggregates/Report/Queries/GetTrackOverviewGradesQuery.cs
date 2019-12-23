using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.Modules;
using Microsoft.Extensions.Configuration;
using Domain.Aggregates.Events;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Levels;

namespace Domain.Aggregates.Report.Queries
{
    public class GetTracksGradesQuery
    {
        public class Contract : IRequest<Result<List<GradeItem>>>
        {
            public string TrackIds { get; set; }
            public string UserRole { get; set; }
        }

        public class GradeItem
        {
            public string Name { get; set; }
            public string Cpf { get; set; }
            public string Track { get; set; }
            public int Ranking { get; set; }
            public int TotalPoints { get; set; }
            public decimal TotalGrades { get; set; }
            public List<TrackModuleGradeItem> ModuleGrade { get; set; }
            public List<TrackEventGradeItem> EventGrade { get; set; }
            public List<TrackEventPresenceItem> EventPresence { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Cpf { get; set; }
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
        public class ModuleGradeItem
        {

            public ObjectId ModuleId { get; set; }
            public string ModuleName { get; set; }
            public List<ModuleUserGradeItem> UserGrades { get; set; }
        }

        public class ModuleUserGradeItem
        {
            public ObjectId UserId { get; set; }
            public decimal Grade { get; set; }
            public int Points { get; set; }
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
                var usersGrades = new List<GradeItem>();
                var selectedTrackIds = new List<string>();

                if (!string.IsNullOrEmpty(appEventGradeKeys))
                {
                    eventGradeKeys = appEventGradeKeys.Split(',').ToList();
                    hasCustomEventGrades = true;
                }

                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<List<GradeItem>>("Acesso Negado");

                if (string.IsNullOrEmpty(request.TrackIds))
                    return Result.Fail<List<GradeItem>>("Id da Trilha não informado");

                selectedTrackIds = request.TrackIds.Split(',').ToList();

                for (int i = 0; i < selectedTrackIds.Count; i++)
                {
                    var trackId = ObjectId.Parse(selectedTrackIds[i]);

                    var dbUsers = await _db.UserCollection
                        .AsQueryable()
                        .Where(
                            x => x.TracksInfo != null &&
                            x.TracksInfo.Any(y => y.Id == trackId)
                        )
                        .Select(x => new UserItem
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Cpf = x.Cpf
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

                    var modulesGrades = GetModulesGrades(userIds, modulesIds);

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
                            Cpf = user.Cpf,
                            Track = dbTrack.Title,
                            TotalPoints = GetUserTotalPoints(userId, modulesGrades),
                            Ranking = GetUserTrackRanking(dbTrack, user.Id),
                            ModuleGrade = GetUserModuleGrade(user.Id, modulesGrades, trackItemsWeight),
                            EventGrade = GetUserEventGrade(user.Id, dbEventApplication, dbEvents, hasCustomEventGrades, eventGradeKeys, trackItemsWeight),
                            EventPresence = GetUserEventPresence(user.Id, dbEventApplication, dbEvents)
                        };

                    usersGrades.AddRange(usersGrade);
                }

                return Result.Ok(usersGrades.ToList());
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
                    foreach (CustomEventGradeValue grade in customGrades)
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
                if (grade.HasValue && weightItem != null && weightItem.Weight > 0)
                {
                    return (grade.Value / 10) * weightItem.Weight;
                }
                return null;
            }

            private int GetUserTrackRanking(Track track, ObjectId userId)
            {
                var query = _db.UserCollection.AsQueryable().Where(x =>
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                ).Select(u => u.Id);

                var studentsIds = ((IAsyncCursorSource<ObjectId>)query).ToList();

                var trackModules = track.ModulesConfiguration.Select(m => m.ModuleId).ToList();

                var progressQuery = _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        trackModules.Contains(x.ModuleId) &&
                        studentsIds.Contains(x.UserId)
                    );

                var moduleUserProgressList = ((IAsyncCursorSource<UserModuleProgress>)progressQuery).ToList();

                var moduleUserProgress = moduleUserProgressList
                    .GroupBy(m => m.UserId)
                    .Select(g => g.Aggregate((acc, x) =>
                    {
                        acc.Points = acc.Points + x.Points;
                        return acc;
                    }))
                    .OrderByDescending(m => m.Points)
                    .ToList();

                int index = moduleUserProgress.FindIndex(m => m.UserId == userId);

                return index >= 0 ? index + 1 : 0;
            }

            public List<ModuleGradeItem> GetModulesGrades(List<ObjectId> userIds, List<ObjectId> moduleIds, DateTimeOffset? cutOffDate = null)
            {
                var moduleGradeList = new List<ModuleGradeItem>();

                if (cutOffDate == null)
                {
                    cutOffDate = DateTimeOffset.MaxValue;
                }

                var modules = _db.ModuleCollection
                   .AsQueryable()
                   .Where(x =>
                       moduleIds.Contains(x.Id)
                   )
                   .ToList();

                var modulesProgress = _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        userIds.Contains(x.UserId) &&
                        moduleIds.Contains(x.ModuleId)
                    )
                    .ToList();

                var subjectProgress = _db.UserSubjectProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        userIds.Contains(x.UserId) &&
                        moduleIds.Contains(x.ModuleId) &&
                        x.Answers.Any(a => a.AnswerDate <= cutOffDate)
                    )
                    .Select(x => new UserSubjectProgressItem
                    {
                        ModuleId = x.ModuleId,
                        UserId = x.UserId,
                        Level = x.Level,
                        Answers = x.Answers.Select(y => y.CorrectAnswer)
                    })
                    .ToList();

                var levelsCount = Level.GetAllLevels().Data.Count();

                foreach (UserModuleProgress prog in modulesProgress)
                {
                    var moduleGrade = moduleGradeList.FirstOrDefault(x => x.ModuleId == prog.ModuleId);
                    var module = modules.FirstOrDefault(x => x.Id == prog.ModuleId);
                    if (module != null)
                    {
                        if (moduleGrade == null)
                        {
                            var newModuleGrade = new ModuleGradeItem
                            {
                                ModuleId = module.Id,
                                ModuleName = module.Title,
                                UserGrades = new List<ModuleUserGradeItem>()
                            };

                            var newModuleUserGrade = new ModuleUserGradeItem
                            {
                                UserId = prog.UserId,
                                Grade = CalculateUserGrade(
                                    module,
                                    prog,
                                    subjectProgress.Where(x => x.ModuleId == prog.ModuleId && x.UserId == prog.UserId).ToList(),
                                    levelsCount
                                ),
                                Points = prog.Points
                            };
                            newModuleGrade.UserGrades.Add(newModuleUserGrade);

                            moduleGradeList.Add(newModuleGrade);
                        }
                        else
                        {
                            var newModuleUserGrade = new ModuleUserGradeItem
                            {
                                UserId = prog.UserId,
                                Grade = CalculateUserGrade(
                                    module,
                                    prog,
                                    subjectProgress.Where(x => x.ModuleId == prog.ModuleId && x.UserId == prog.UserId).ToList(),
                                    levelsCount
                                )
                            };
                            moduleGrade.UserGrades.Add(newModuleUserGrade);
                        }
                    }
                }


                return moduleGradeList;
            }

            private decimal CalculateUserGrade(Module module, UserModuleProgress moduleProgress,
            List<UserSubjectProgressItem> userProgress, decimal levelsCount)
            {
                switch (module.ModuleGradeType)
                {
                    case ModuleGradeTypeEnum.Percentage:
                        decimal badgeValue = 0;
                        if (moduleProgress == null)
                        {
                            return badgeValue;
                        }
                        switch (moduleProgress.Level)
                        {
                            case 1:
                                badgeValue = (decimal)0.3;
                                break;
                            case 2:
                                badgeValue = (decimal)0.6;
                                break;
                            case 3:
                                badgeValue = (decimal)0.9;
                                break;
                            case 4:
                                badgeValue = (decimal)1.0;
                                break;
                        }
                        var totalAnswers = new List<bool>();
                        foreach (List<bool> userAnswerList in userProgress.Select(x => x.Answers).ToList())
                        {
                            totalAnswers.AddRange(userAnswerList);
                        }
                        var correctAnswers = totalAnswers.Where(x => x).ToList();
                        if (correctAnswers.Count == 0 || totalAnswers.Count == 0 || badgeValue == 0)
                        {
                            return 0;
                        }
                        return ((decimal)correctAnswers.Count / (totalAnswers.Count > 0 ? totalAnswers.Count : 1)) *
                            badgeValue;

                    case ModuleGradeTypeEnum.SubjectsLevel:
                    default:
                        if (userProgress == null || userProgress.Count == 0)
                        {
                            return 0;
                        }
                        var levelSum = userProgress.Select(x => x.Level).Sum();
                        if (levelSum == 0 || module.Subjects == null || module.Subjects.Count == 0 || levelsCount == 0)
                        {
                            return 0;
                        }
                        return levelSum / (module.Subjects.Count * levelsCount);
                }
            }

            private int GetUserTotalPoints(ObjectId userId, List<ModuleGradeItem> moduleGrades)
            {
                var userModuleGrades = moduleGrades.Where(x => x.UserGrades.Any(g => g.UserId == userId))
                    .Select(x => x.UserGrades).ToList();
                int userPoints = 0;

                for (int i = 0; i < userModuleGrades.Count; i++)
                {
                    var currentGrade = userModuleGrades[i].Where(x => x.UserId == userId)
                        .GroupBy(x => x.UserId)
                        .Select(g => g.Sum(x => x.Points)).First();

                    if(currentGrade > 0)
                    {
                        var teste = currentGrade;
                    }

                    userPoints += currentGrade;
                }

                return userPoints;
            }


            private decimal GetUserTotalGrades(ObjectId userId, List<ModuleGradeItem> moduleGrades)
            {
                var userModuleGrades = moduleGrades.Where(x => x.UserGrades.Any(g => g.UserId == userId))
                    .Select(x => x.UserGrades).ToList();
                decimal userGrades = 0;

                for (int i = 0; i < userModuleGrades.Count; i++)
                {
                    var currentGrade = userModuleGrades[i].Where(x => x.UserId == userId)
                        .GroupBy(x => x.UserId)
                        .Select(g => g.Sum(x => x.Grade)).First();

                    userGrades += currentGrade;
                }

                return userGrades;
            }
        }

    }
}

