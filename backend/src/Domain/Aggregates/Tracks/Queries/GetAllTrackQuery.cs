using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetAllTrackQuery
    {
        public class Contract : CommandContract<Result<List<TrackItem>>>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }

        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }



        public class Handler : IRequestHandler<Contract, Result<List<TrackItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<TrackItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var trackCollection = _db.Database.GetCollection<TrackItem>("Tracks");
                    var documents = await trackCollection.Find(Builders<TrackItem>.Filter.Empty).ToListAsync();

                    return Result.Ok(documents);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<TrackItem>>($"Ocorreu um erro ao buscar as questões: {err.Message}");
                }
            }
        }
    }
}
