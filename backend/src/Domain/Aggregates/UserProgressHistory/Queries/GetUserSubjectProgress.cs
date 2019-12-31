using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
//ROLLBACK BDQ
namespace Domain.Aggregates.UserProgressHistory.Queries
{
    public class GetUserSubjectProgress
    {
        public class Contract : CommandContract<Result<SubjectProgressInfo>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
        }

        public class SubjectProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
            //public decimal PassPercentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<SubjectProgressInfo>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<SubjectProgressInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var userId = ObjectId.Parse(request.UserId);
                    var subjectId = ObjectId.Parse(request.SubjectId);
                    var progress = await _db
                        .UserSubjectProgressCollection
                        .AsQueryable()
                        .Where(x => x.ModuleId == moduleId &&
                                    x.SubjectId == subjectId &&
                                    x.UserId == userId)
                        .Select(x => new SubjectProgressInfo()
                        {
                            Level = x.Level,
                            ModuleId = x.ModuleId,
                            SubjectId = x.SubjectId,
                            //PassPercentage = x.PassPercentage,
                            Progress = x.Progress
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (progress == null)
                    {
                        progress = new SubjectProgressInfo()
                        {
                            Level = Level.GetAllLevels().Data.First().Id,
                            Progress = 0,
                            ModuleId = moduleId,
                            //PassPercentage = 1,
                            SubjectId = subjectId
                        };
                    }

                    return Result.Ok(progress);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

        }
    }
}
