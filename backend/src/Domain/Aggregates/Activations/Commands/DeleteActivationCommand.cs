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

namespace Domain.Aggregates.Activations.Commands
{
    public class DeleteActivationCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string ActivationId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var actId = ObjectId.Parse(request.ActivationId);

                var act = await _db.ActivationsCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == actId);

                if (act == null)
                    return Result.Fail<Contract>("Objeto não encontrado");

                await _db.ActivationsCollection.DeleteOneAsync(
                    t => t.Id == actId,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
