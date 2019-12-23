using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class BlockUser
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>{
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student")
                    return Result.Fail("Acesso Negado");

                var userQry = await _db.UserCollection.FindAsync(u => u.Id == ObjectId.Parse(request.Id), cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                user.IsBlocked = !user.IsBlocked;

                await _db.UserCollection.ReplaceOneAsync(t => t.Id == user.Id, user,
                       cancellationToken: cancellationToken);

                return Result.Ok(request);
            }
        }
    }
}
