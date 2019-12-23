using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Aggregates.UserProgressHistory.Commands;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserProgressHistory.Queries
{
    public class GetUserModuleSubjectsProgressQuery
    {
        public class Contract : CommandContract<Result<ModuleProgressInfo>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId Id { get; set; }
            public List<Requirement> Requirements { get; set; }
        }

        public class ModuleProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
            public List<SubjectProgressInfo> SubjectsProgress { get; set; }
            public List<RequirementsProgressInfo> RequirementsProgress { get; set; }
        }

        public class SubjectProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
        }

        public class RequirementsProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<ModuleProgressInfo>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<ModuleProgressInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.ModuleId))
                        return Result.Fail<ModuleProgressInfo>("Id do Módulo não informado");

                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var userId = ObjectId.Parse(request.UserId);

                    var module = await GetModuleById(moduleId, cancellationToken);
                    if (module == null)
                        return Result.Fail<ModuleProgressInfo>("Módulo não existe");

                    var progress = await _db
                        .UserSubjectProgressCollection
                        .AsQueryable()
                        .Where(x => x.ModuleId == moduleId &&
                                    x.UserId == userId)
                        .Select(x=> new SubjectProgressInfo()
                        {
                            Level = x.Level,
                            ModuleId = x.ModuleId,
                            SubjectId = x.SubjectId,
                            Progress = x.Progress
                        })
                        .ToListAsync(cancellationToken);

                    var moduleProgress = await _db
                        .UserModuleProgressCollection
                        .AsQueryable()
                        .Where(x => x.ModuleId == moduleId &&
                                    x.UserId == userId)
                        .Select(x => new ModuleProgressInfo()
                        {
                            Level = x.Level,
                            ModuleId = x.ModuleId,
                            Progress = x.Progress
                        })
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (moduleProgress == null)
                    {
                        moduleProgress = new ModuleProgressInfo()
                        {
                            Level = Level.GetAllLevels().Data.First().Id,
                            Progress = 0,
                            ModuleId = moduleId
                        };
                    }

                    moduleProgress.SubjectsProgress = progress;
                    moduleProgress.RequirementsProgress = new List<RequirementsProgressInfo>();

                    foreach (var requirement in module.Requirements)
                    {
                        var reqProgress = await GetModuleProgressById(
                            requirement.ModuleId, userId, cancellationToken
                        );
                        
                        if (reqProgress != null)
                        {
                            moduleProgress.RequirementsProgress.Add(new RequirementsProgressInfo {
                                ModuleId = requirement.ModuleId,
                                Level = reqProgress.Level,
                                Progress = reqProgress.Progress
                            });
                        }
                    }

                    return Result.Ok(moduleProgress);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

            private async Task<ModuleItem> GetModuleById(ObjectId moduleId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<ModuleItem>("Modules")
                    .FindAsync(x => x.Id == moduleId);

                return await query.FirstOrDefaultAsync();
            }

            private async Task<UserModuleProgress> GetModuleProgressById(ObjectId moduleId, ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<UserModuleProgress>("UserModuleProgress")
                    .FindAsync(x =>
                        x.ModuleId == moduleId &&
                        x.UserId == userId
                    );

                return await query.FirstOrDefaultAsync();
            }
        }
    }
}
