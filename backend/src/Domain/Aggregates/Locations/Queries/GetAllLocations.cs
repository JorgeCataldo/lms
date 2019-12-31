using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.GetAllLocations.Queries
{
    public class GetAllLocations
    {
        public class Contract : IRequest<Result<List<LocationItem>>>
        {
            
        }

        public class LocationItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public ObjectId CountryId { get; set; }
        }


        public class Handler : IRequestHandler<Contract, Result<List<LocationItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<LocationItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var collection = _db.Database.GetCollection<LocationItem>("Locations");

                var qry = await collection.FindAsync(FilterDefinition<LocationItem>.Empty,                    
                    cancellationToken: cancellationToken
                );

                var locations = await qry.ToListAsync(cancellationToken);

                return Result.Ok(locations);
            }
        }
    }
}
