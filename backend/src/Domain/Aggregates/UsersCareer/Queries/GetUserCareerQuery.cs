using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UsersCareer.Queries
{
    public class GetUserCareerQuery
    {
        public class Contract : CommandContract<Result<UserCareer>>
        {
            public string UserId { get; set; }
            public string CurrentUserRole { get; set; }
            public string CurrentUserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<UserCareer>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<UserCareer>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole == "Secretary")
                    return Result.Fail<UserCareer>("Acesso Negado");

                if (request.CurrentUserRole == "Student" && request.CurrentUserId != request.UserId)
                    return Result.Fail<UserCareer>("Acesso Negado");

                var user = await GetUserCareer(request, cancellationToken);

                if (user == null)
                    return Result.Fail<UserCareer>("Sem Dados");

                return Result.Ok(user);
            }

            private async Task<UserCareer> GetUserCareer(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);

                return await _db.UserCareerCollection
                    .AsQueryable()
                    .Where(x => x.CreatedBy == userId)
                    .FirstOrDefaultAsync(cancellationToken: token);
            }
        }
    }
}
