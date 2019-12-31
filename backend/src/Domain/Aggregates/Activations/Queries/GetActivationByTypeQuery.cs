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
    public class GetActivationByTypeQuery
    {
        public class Contract : CommandContract<Result<Activation>>
        {
            public ActivationTypeEnum Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Activation>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<Activation>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var act = await _db.ActivationsCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Type == request.Type);

                if (act == null)
                    return Result.Ok(Activation.Create().Data);

                return Result.Ok(act);
            }
        }
    }
}
