using System;
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

namespace Domain.Aggregates.RecruitingCompany.Queries
{
    public class GetAvailableCandidates
    {
        public class Contract : CommandContract<Result<List<UserItem>>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class UserItem
        {
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public bool IsFavorite { get; set; } = false;
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
                if (request.UserRole != "Recruiter" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<List<UserItem>>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var favoritesIds = await GetFavorites(userId);

                var users = await _db.UserCollection
                    .AsQueryable()
                    .Where(u =>
                        u.Id != userId &&
                        !u.IsBlocked &&
                        u.AllowRecommendation.HasValue &&
                        u.AllowRecommendation.Value &&
                        u.SecretaryAllowRecommendation.HasValue &&
                        u.SecretaryAllowRecommendation.Value
                    )
                    .OrderByDescending(x =>
                        x.CreatedAt
                    )
                    .Select(u => new UserItem {
                        ImageUrl = u.ImageUrl,
                        Name = u.Name,
                        IsFavorite = favoritesIds.Contains(u.Id)
                    }).ToListAsync();

                return Result.Ok(users);
            }

            private async Task<List<ObjectId>> GetFavorites(ObjectId recruiterId)
            {
                var queryable = _db.RecruitmentFavoriteCollection.AsQueryable();

                return await queryable
                    .Where(f => f.RecruiterId == recruiterId)
                    .Select(f => f.UserId)
                    .ToListAsync();
            }
        }
    }
}
