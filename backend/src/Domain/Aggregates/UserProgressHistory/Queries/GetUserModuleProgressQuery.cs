using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserProgressHistory.Queries
{
    public class GetUserModuleProgressQuery
    {
        public class Contract : CommandContract<Result<List<ModuleProgressInfo>>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
        }

        public class ModuleProgressInfo
        {
            public ObjectId ModuleId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<ModuleProgressInfo>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<List<ModuleProgressInfo>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = ObjectId.Parse(request.UserId);

                    var moduleProgress = await _db
                        .UserModuleProgressCollection
                        .AsQueryable()
                        .Where(x => x.UserId == userId)
                        .Select(x => new ModuleProgressInfo()
                        {
                            Level = x.Level,
                            ModuleId = x.ModuleId,
                            Progress = x.Progress
                        })
                        .ToListAsync(cancellationToken: cancellationToken);
                    
                    return Result.Ok(moduleProgress);
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
