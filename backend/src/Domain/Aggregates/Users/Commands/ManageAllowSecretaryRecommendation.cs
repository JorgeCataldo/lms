using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class ManageAllowSecretaryRecommendation
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserRole { get; set; }
            public string UserId { get; set; }
            public bool AllowRecommendation { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail("Acesso Negado");

                if (request.CurrentUserRole != "Secretary" && request.CurrentUserRole != "Admin" && request.CurrentUserRole != "HumanResources" && request.CurrentUserRole != "Recruiter")
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var user = await _db.UserCollection.AsQueryable()
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                user.SecretaryAllowRecommendation = request.AllowRecommendation;

                await _db.UserCollection.ReplaceOneAsync(
                    u => u.Id == userId, user,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
