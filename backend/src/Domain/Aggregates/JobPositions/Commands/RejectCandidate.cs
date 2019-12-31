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
    public class RejectCandidate
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserRole { get; set; }
            public string JobPositionId { get; set; }
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
                if (request.CurrentUserRole != "Recruiter" && request.CurrentUserRole != "HumanResources")
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var candidate = await _db.CandidateCollection.AsQueryable()
                    .Where(c => c.UserId == userId && c.JobPositionId == positionId)
                    .FirstOrDefaultAsync();

                candidate.Approved = false;

                await _db.CandidateCollection.ReplaceOneAsync(c =>
                    c.UserId == userId && c.JobPositionId == positionId,
                    candidate, cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
