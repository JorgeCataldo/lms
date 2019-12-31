using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Activations.Queries
{
    public class GetActivationsQuery
    {
        public class Contract : CommandContract<Result<List<Activation>>>
        {
            public ActivationTypeEnum Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<Activation>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<Activation>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var act = await _db.ActivationsCollection
                    .AsQueryable()
                    .ToListAsync();

                if (act == null)
                    return Result.Ok(new List<Activation>());

                return Result.Ok(act);
            }
        }
    }
}
