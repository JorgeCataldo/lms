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
    public class SetFirstAccessStatusCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public DateTimeOffset LimitDate { get; set; }
            public bool Status { get; set; }
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
                var QryList = await _db.Database.GetCollection<Student>("Users")
                    .AsQueryable()
                    .Where(x => x.CreatedAt < request.LimitDate && !x.IsBlocked)
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();

                foreach (User user in QryList)
                {
                    user.FirstAccess = request.Status;
                    await _db.UserCollection.ReplaceOneAsync(
                        t => t.Id == user.Id, user,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(request);
            }
        }
    }
}
