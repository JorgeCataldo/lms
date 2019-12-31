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
namespace Domain.Aggregates.RecruitmentFavorite.Commands
{
    public class RemoveRecruitmentFavorite
    {
        public class Contract : CommandContract<Result>
        {
            public string RecruiterRole { get; set; }
            public string RecruiterId { get; set; }
            public string UserId { get; set; }
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
                if (request.RecruiterRole != "Recruiter" && request.RecruiterRole != "HumanResources") 
                    return Result.Fail<Result>("Acesso Negado");

                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var recruiterId = ObjectId.Parse(request.RecruiterId);

                await _db.RecruitmentFavoriteCollection
                    .DeleteOneAsync(f =>
                        f.RecruiterId == recruiterId &&
                        f.UserId == userId
                    );

                return Result.Ok();
            }
        }
    }
}
