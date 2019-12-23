using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class AdminChangePassword
    {
        public class Contract: CommandContract<Result>
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string NewPassword { get; set; }

            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IEmailProvider provider, IOptions<DomainOptions> options, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                var userQry = await _db.UserCollection.FindAsync(u => u.Id == ObjectId.Parse(request.UserId), cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pass = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!pass.Succeeded)
                    return Result.Fail(pass.Errors.ToList()[0].Description);

                return Result.Ok();
            }
        }
    }
}