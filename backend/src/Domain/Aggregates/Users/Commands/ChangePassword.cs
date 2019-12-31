using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class ChangePassword
    {
        public class Contract : CommandContract<Result>
        {
            public string UserId { get; set; }

            [Required]
            public string CurrentPassword { get; set; }

            [Required]
            public string NewPassword { get; set; }
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
                var userIdResult = ObjectId.Parse(request.UserId);
                var userQry = await _db.UserCollection.FindAsync(u => u.Id == userIdResult, cancellationToken: cancellationToken );
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                    return Result.Fail(result.Errors.First()?.Description ?? "Ocorreu um erro ao trocar a senha");

                return Result.Ok();
            }
        }
    }
}
