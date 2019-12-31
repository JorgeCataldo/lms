using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
namespace Domain.Aggregates.JobPosition.Queries
{
    public class GetCandidateAcceptedStatus
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
                if (request.CurrentUserRole != "Student")
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.CurrentUserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var candidate = await _db.CandidateCollection.AsQueryable()
                    .FirstOrDefaultAsync(c =>
                        c.JobPositionId == positionId &&
                        c.UserId == userId
                    );

                if (candidate == null)
                    return Result.Ok(Candidate.Candidate.Create(
                        positionId, userId, "", userId
                    ).Data);

                return Result.Ok(candidate);
            }
        }
    }
}
