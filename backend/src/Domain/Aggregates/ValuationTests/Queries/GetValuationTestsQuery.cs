using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetValuationTestsQuery
    {
        public class Contract : CommandContract<Result<List<ValuationTestItem>>>
        {
            public string UserRole { get; set; }
        }

        public class ValuationTestItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ValuationTestItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ValuationTestItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "Admin")
                    return Result.Fail<List<ValuationTestItem>>("Acesso Negado");

                var options = new FindOptions<ValuationTestItem>() {
                    Sort = Builders<ValuationTestItem>.Sort.Ascending("Title")
                };
                
                var collection = _db.Database.GetCollection<ValuationTestItem>("ValuationTests");
                var query = await collection.FindAsync(
                    FilterDefinition<ValuationTestItem>.Empty,
                    options: options,
                    cancellationToken: cancellationToken
                );
                
                return Result.Ok(
                    await query.ToListAsync(cancellationToken)
                );
            }
        }
    }
}
