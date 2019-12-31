using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class ResetPassword
    {
        public class Contract : CommandContract<Result>{
            public string Username { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, UserManager<User> userManager){
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userQry = await _db.UserCollection.FindAsync(u => u.UserName == request.Username, cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

                if (!result.Succeeded) 
                    return Result.Fail(result.Errors?.FirstOrDefault()?.Description ?? "Erro não identificado");

                return Result.Ok();
            }
        }
    }
}
