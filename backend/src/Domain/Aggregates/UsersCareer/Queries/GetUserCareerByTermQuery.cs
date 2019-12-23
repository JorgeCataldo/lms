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
using System.Collections.Generic;
using Domain.Aggregates.Users;

namespace Domain.Aggregates.UsersCareer.Queries
{
    public class GetUserCareerByTermQuery
    {
        public class Contract : CommandContract<Result<List<UserItem>>>
        {
            public string Term { get; set; }
            public string DDD { get; set; }
            public string Cidade { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Phone2 { get; set; }
            //public string Objective { get; set; }
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
                var carrers = await GetUserIds(request, cancellationToken);
                var userIds = carrers.Select(x => x.Id).ToList();

                if (userIds.Count == 0)
                    return Result.Fail<List<UserItem>>("Sem Dados");

                var collection = _db.Database.GetCollection<User>("Users");
                var queryable = collection.AsQueryable();
                
                var users = await queryable
                    .Where(u => (userIds.Contains(u.Id)))
                    .Select(u => new UserItem
                    {
                        Id = u.Id,                        
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.Phone,
                        Phone2 = u.Phone2
                    }).ToListAsync();

                return Result.Ok(users);
            }

            private async Task<List<UserCareer>> GetUserIds(Contract request, CancellationToken token)
            {
                return await _db.UserCareerCollection
                    .AsQueryable()
                    .Where(x => x.ShortDateObjectives.ToUpper().Contains(request.Term.ToUpper()))
                    .ToListAsync(cancellationToken: token);
            }
        }
    }
}
