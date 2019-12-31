using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Institutions.Institute;

namespace Domain.Aggregates.UsersCareer.Queries
{
    public class GetUserInstitutionsQuery
    {
        public class Contract : CommandContract<Result<List<InstituteItem>>>
        {
            public string Name { get; set; }
        }

        public class InstituteItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public InstituteType Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<InstituteItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<InstituteItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var options = new FindOptions<InstituteItem>() { Limit = 5 };

                FilterDefinition<InstituteItem> filters = SetFilters(request);

                var collection = _db.Database.GetCollection<InstituteItem>("Institutions");
                var query = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );
                var list = await query.ToListAsync(cancellationToken);

                return Result.Ok(list);
            }

            private FilterDefinition<InstituteItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<InstituteItem>.Empty;
                var builder = Builders<InstituteItem>.Filter;

                if (!String.IsNullOrEmpty(request.Name))
                {
                    filters = filters & builder.Regex(
                        "name", new BsonRegularExpression("/" + request.Name + "/is")
                    );
                }

                return filters;
            }
        }
    }
}
