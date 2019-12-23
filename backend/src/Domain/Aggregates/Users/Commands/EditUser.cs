using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class EditUser
    {
        public class Contract : CommandContract<Result>{
            public string UserId { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string Cpf { get; set; }

            public Address Address { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>{
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userQry = await _db.UserCollection.FindAsync(u => u.Id == ObjectId.Parse(request.UserId), cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                var cpfResult = Cpf.Create(request.Cpf);
                if (cpfResult.IsFailure)
                    return cpfResult;

                var editResult = user.Edit(request.Name, request.Email, request.Cpf, request.Address);
                if (editResult.IsFailure)
                    return editResult;

                var filter = Builders<User>.Filter.Eq(p => p.Id, user.Id);
                await _db.UserCollection.ReplaceOneAsync(filter, user, cancellationToken: cancellationToken );

                return Result.Ok();
            }
        }
    }
}
