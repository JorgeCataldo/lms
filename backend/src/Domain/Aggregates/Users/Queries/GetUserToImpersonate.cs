using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserToImpersonate
    {
        public class Contract : CommandContract<Result<UserPreviewInterceptor>>{
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class UserPreviewInterceptor
        {
            public ObjectId UserId;
            public string UserRole;
            public string UserImgUrl;
            public string UserName;
        }

        public class Handler : IRequestHandler<Contract, Result<UserPreviewInterceptor>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, UserManager<User> userManager){
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<UserPreviewInterceptor>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" || string.IsNullOrEmpty(request.UserId))
                {
                    return Result.Fail<UserPreviewInterceptor>("Acesso Negado");
                }

                var userId = ObjectId.Parse(request.UserId);

                var userToImpersonate = await _db.UserCollection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (userToImpersonate == null)
                {
                    return Result.Fail<UserPreviewInterceptor>("Usuário inexistente");
                }

                var user = new UserPreviewInterceptor
                {
                    UserId = userToImpersonate.Id,
                    UserRole = GetUserType(userToImpersonate),
                    UserImgUrl = userToImpersonate.ImageUrl,
                    UserName = userToImpersonate.UserName
                };

                return Result.Ok(user);
            }

            private string GetUserType(User user)
            {
                if (user is Admin)
                    return "Admin";
                if (user is Recruiter)
                    return "Recruiter";
                if (user is HumanResources)
                    return "HumanResources";
                if (user is Secretary)
                    return "Secretary";
                else
                    return "Student";
            }
        }
    }
}
