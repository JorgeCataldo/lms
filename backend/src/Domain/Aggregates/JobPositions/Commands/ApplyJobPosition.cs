using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
namespace Domain.Aggregates.JobPosition.Commands
{
    public class ApplyJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserId { get; set; }
            public string CurrentUserRole { get; set; }
            public string JobPositionId { get; set; }
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
                if (request.CurrentUserRole != "Student" && request.CurrentUserRole != "Admin")
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.CurrentUserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var candidate = await _db.CandidateCollection.AsQueryable()
                    .FirstOrDefaultAsync(c =>
                        c.JobPositionId == positionId &&
                        c.UserId == userId
                    );

                if (candidate != null)
                    return Result.Fail<Result>("Ususário ja é um candidato da vaga");

                var user = await _db.UserCollection.AsQueryable()
                    .FirstOrDefaultAsync(u =>
                        u.Id == userId
                    );

                if (user == null)
                    return Result.Fail<Result>("Ususário não existe");

                var newCandidate = Candidate.Candidate.Create(
                        positionId, userId, user.UserName, userId
                    ).Data;

                await _db.CandidateCollection.InsertOneAsync(
                        newCandidate, cancellationToken: cancellationToken
                    );

                return Result.Ok();
            }
        }
    }
}
