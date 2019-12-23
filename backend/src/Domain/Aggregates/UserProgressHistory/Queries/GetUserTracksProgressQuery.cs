using System;
using System.Collections.Generic;
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
    public class GetUserTracksProgressQuery
    {
        
        public class Contract : CommandContract<Result<List<TrackProgressInfo>>>
        {
            public string UserId { get; set; }
        }

        public class TrackProgressInfo
        {
            public ObjectId TrackId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
            public List<ObjectId> ModulesCompleted { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<TrackProgressInfo>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<List<TrackProgressInfo>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.UserId))
                    return Result.Fail<List<TrackProgressInfo>>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var trackProgress = await _db
                    .UserTrackProgressCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId)
                    .Select(x => new TrackProgressInfo()
                    {
                        Level = x.Level,
                        TrackId = x.TrackId,
                        Progress = x.Progress,
                        ModulesCompleted = x.ModulesCompleted
                    })
                    .ToListAsync(cancellationToken: cancellationToken);
                    
                return Result.Ok(trackProgress);
            }
        }
    }
}
