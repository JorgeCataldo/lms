using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class SetSeeHowCommand
    {
        public class Contract : CommandContract<Result<string>>
        {
            public string UserRoleToChange { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<string>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<string>> Handle(Contract request, CancellationToken cancellationToken)
            {
                return Result.Ok(request.UserRoleToChange);
            }
        }
    }
}
