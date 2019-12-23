using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests.Queries
{
    public class GetProfileTestsQuery
    {
        public class Contract : CommandContract<Result<List<ProfileTestItem>>>
        {
            public string UserRole { get; set; }
        }

        public class ProfileTestItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ProfileTestItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ProfileTestItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail<List<ProfileTestItem>>("Acesso Negado");

                var options = new FindOptions<ProfileTestItem>() {
                    Sort = Builders<ProfileTestItem>.Sort.Ascending("Title")
                };
                
                var collection = _db.Database.GetCollection<ProfileTestItem>("ProfileTests");
                var query = await collection.FindAsync(
                    FilterDefinition<ProfileTestItem>.Empty,
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
