using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class GetProfessorsQuery
    {
        public class Contract : CommandContract<Result<List<User>>>
        {
            public string CurrentUserRole { get; set; }
            public string Term { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<User>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<User>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Admin" &&
                    request.CurrentUserRole != "HumanResources" &&
                    request.CurrentUserRole != "Secretary" && request.CurrentUserRole != "Recruiter")
                {
                    return Result.Fail<List<User>>("Acesso Negado");
                }
                try
                {
                    var options = new FindOptions<User>() { Limit = 5 };

                    FilterDefinition<User> filters = SetFilters(request);

                    var collection = _db.Database.GetCollection<User>("Users");
                    var query = await collection.FindAsync(filters,
                        options: options,
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok(
                        await query.ToListAsync(cancellationToken)
                    );

                }
                catch (Exception err)
                {
                    return Result.Fail<List<User>>(
                        $"Ocorreu um erro ao buscar os usuarios: {err.Message}"
                    );
                }

            }

            private FilterDefinition<User> SetFilters(Contract request)
            {
                var filters = FilterDefinition<User>.Empty;
                var builder = Builders<User>.Filter;

                filters = filters & builder.Where(x => !x.IsBlocked);

                if (!String.IsNullOrEmpty(request.Term))
                {
                    filters = filters & builder.Regex(
                        "name", new BsonRegularExpression("/" + request.Term + "/is")
                    );
                }

                return filters;
            }
        }
    }
}
