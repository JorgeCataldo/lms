using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class ExportUsersGradeQuery
    {

        public class Contract : CommandContract<Result<List<GradeItem>>>
        {
            public List<string> UserIds { get; set; }
            public string UserRole { get; set; }
        }

        public class GradeItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public List<ModuleGradeItem> ModuleGrade { get; set; }
            public List<EventGradeItem> EventGrade { get; set; }
            public List<EventPresenceItem> EventPresence { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
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
        }

        public class ModuleGradeItem
        {
            public string ModuleName { get; set; }
            public int Points { get; set; }
        }

        public class EventGradeItem
        {
            public string EventName { get; set; }
            public decimal? FinalGrade { get; set; }
        }

        public class EventPresenceItem
        {
            public string EventName { get; set; }
            public bool? UserPresence { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<GradeItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<GradeItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student")
                    return Result.Fail<List<GradeItem>>("Acesso Negado");
                if (request.UserIds == null || request.UserIds.Count == 0)
                    return Result.Fail<List<GradeItem>>("Lista de usuários inválida");

                var gradeItems = new List<GradeItem>();

                List<ObjectId> userIds = new List<ObjectId>();
                foreach (string userId in request.UserIds)
                {
                    ObjectId objId = new ObjectId();
                    if (ObjectId.TryParse(userId, out objId))
                        userIds.Add(objId);
                }

                var dbUsers = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => userIds.Contains(x.Id))
                    .Select(x => new UserItem {
                        Id = x.Id,
                        Name = x.Name,
                        Email = x.Email
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var dbModuleProgress = await _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x => userIds.Contains(x.UserId))
                    .Select(x => new ModuleProgressItem {
                        ModuleId = x.ModuleId,
                        UserId = x.UserId,
                        Points = x.Points
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var modulesIds = dbModuleProgress.Select(x => x.ModuleId).ToList();

                var dbModules = await _db.ModuleCollection
                    .AsQueryable()
                    .Where(x => modulesIds.Contains(x.Id))
                    .Select(x => new RelationalItem
                    {
                        Id = x.Id,
                        Name = x.Title
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var dbEventApplication = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => userIds.Contains(x.UserId))
                    .Select(x => new EventApplicationItem
                    {
                        EventId = x.EventId,
                        UserId = x.UserId,
                        OrganicGrade = x.OrganicGrade,
                        InorganicGrade = x.InorganicGrade,
                        UserPresence = x.UserPresence
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var eventsIds = dbEventApplication.Select(x => x.EventId).ToList();

                var dbEvents = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => eventsIds.Contains(x.Id))
                    .Select(x => new RelationalItem
                    {
                        Id = x.Id,
                        Name = x.Title
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                var usersGrade =
                    from userId in userIds
                    join user in dbUsers on userId equals user.Id
                    select new GradeItem
                    {
                        Name = user.Name,
                        Email = user.Email,
                        ModuleGrade = GetUserModuleGrade(user.Id, dbModuleProgress, dbModules),
                        EventGrade = GetUserEventGrade(user.Id, dbEventApplication, dbEvents),
                        EventPresence = GetUserEventPresence(user.Id, dbEventApplication, dbEvents)
                    };

                return Result.Ok(usersGrade.ToList());
            }

            private List<ModuleGradeItem> GetUserModuleGrade (ObjectId userId, List<ModuleProgressItem> modProgs, List<RelationalItem> mods)
            {
                return (from modProg in modProgs
                        join mod in mods on modProg.ModuleId equals mod.Id
                        where modProg.UserId == userId
                        select new ModuleGradeItem
                        {
                            ModuleName = mod.Name,
                            Points = modProg.Points
                        }).ToList();
            }

            private List<EventGradeItem> GetUserEventGrade(ObjectId userId, List<EventApplicationItem> eveApps, List<RelationalItem> eves)
            {
                return (from eveApp in eveApps
                        join eve in eves on eveApp.EventId equals eve.Id
                        where eveApp.UserId == userId
                        select new EventGradeItem
                        {
                            EventName = eve.Name,
                            FinalGrade = GetFinalGrade(eveApp)
                        }).ToList();
            }

            private List<EventPresenceItem> GetUserEventPresence(ObjectId userId, List<EventApplicationItem> eveApps, List<RelationalItem> eves)
            {
                return (from eveApp in eveApps
                        join eve in eves on eveApp.EventId equals eve.Id
                        where eveApp.UserId == userId
                        select new EventPresenceItem
                        {
                            EventName = "Presença - " + eve.Name,
                            UserPresence = eveApp.UserPresence
                        }).ToList();
            }

            private decimal? GetFinalGrade(EventApplicationItem application)
            {
                if (application != null && application.InorganicGrade.HasValue && application.OrganicGrade.HasValue)
                {
                    var sum = application.InorganicGrade.Value + application.OrganicGrade.Value;
                    return Math.Round(sum / 2, 2);
                }
                return null;
            }
        }
    }
}
