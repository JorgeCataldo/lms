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
    public class GetUserTrackProgressQuery
    {
        
        public class Contract : CommandContract<Result<TrackProgressInfo>>
        {
            public string UserId { get; set; }
            public string TrackId { get; set; }
        }

        public class TrackProgressInfo
        {
            public ObjectId TrackId { get; set; }
            public int Level { get; set; }
            public decimal Progress { get; set; }
            public List<ObjectId> ModulesCompleted { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<TrackProgressInfo>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<TrackProgressInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<TrackProgressInfo>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var trackId = ObjectId.Parse(request.TrackId);

                var trackProgress = await _db
                    .UserTrackProgressCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId && x.TrackId == trackId)
                    .Select(x => new TrackProgressInfo()
                    {
                        Level = x.Level,
                        TrackId = x.TrackId,
                        Progress = x.Progress,
                        ModulesCompleted = x.ModulesCompleted
                    })
                    .FirstOrDefaultAsync();

                if (trackProgress == null)
                {
                    //var published = await _db
                    //.TrackCollection
                    //.AsQueryable()
                    //.Where(x => x.Id == trackId)
                    //.Select(x => x.Published)
                    //.FirstOrDefaultAsync();

                    //if (published)
                    //{
                        var newTrackProgress = new UserTrackProgress(trackId, userId, 0, 0);
                        await _db.UserTrackProgressCollection.InsertOneAsync(
                            newTrackProgress,
                            cancellationToken: cancellationToken
                        );
                        return Result.Ok(new TrackProgressInfo()
                        {
                            Level = newTrackProgress.Level,
                            TrackId = newTrackProgress.TrackId,
                            Progress = newTrackProgress.Progress,
                            ModulesCompleted = newTrackProgress.ModulesCompleted
                        });
                    //}
                }
                    
                return Result.Ok(trackProgress);
            }
        }
    }
}
