using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UsersCareer.Queries
{
    public class ExportUsersCareerQuery
    {
        public class Contract : CommandContract<Result<List<UserItem>>>
        {
            public List<UsersToExport> Users { get; set; }
            public string UserRole { get; set; }
        }

        public class UsersToExport
        {
            public string Name { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
        }

        public class UserItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public UserCareer Career { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<UserItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<UserItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student")
                    return Result.Fail<List<UserItem>>("Acesso Negado");

                if (request.Users == null || request.Users.Count == 0)
                    return Result.Fail<List<UserItem>>("Lista de usuários inválida");

                var users = await GetUsersCareer(request, cancellationToken);

                return Result.Ok(users);
            }

            private async Task<List<UserItem>> GetUsersCareer(Contract request, CancellationToken token)
            {
                List<ObjectId> userIds = request.Users.Select(u =>
                    ObjectId.Parse(u.UserId)
                ).ToList();

                var careers = await _db.UserCareerCollection
                    .AsQueryable()
                    .Where(x => userIds.Contains(x.CreatedBy))
                    .ToListAsync();
                                
                return request.Users.Select(u => new UserItem
                {
                    Name = u.Name,
                    Email = u.Email,
                    Career = careers.Find(c => c.CreatedBy == ObjectId.Parse(u.UserId))
                }).ToList();                
            }
        }
    }
}
