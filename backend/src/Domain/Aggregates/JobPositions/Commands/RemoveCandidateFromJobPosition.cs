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

namespace Domain.Aggregates.JobPosition.Commands
{
    public class RemoveCandidateFromJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserRole { get; set; }
            public string UserId { get; set; }
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
                if (request.CurrentUserRole != "Recruiter" && request.CurrentUserRole != "HumanResources")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail("Usuário não informado");

                if (String.IsNullOrEmpty(request.JobPositionId))
                    return Result.Fail("Vaga não informada");

                var userId = ObjectId.Parse(request.UserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var hasApplication = await _db.CandidateCollection.AsQueryable()
                    .Where(p => p.JobPositionId == positionId && p.UserId == userId)
                    .AnyAsync();

                if (!hasApplication)
                    return Result.Fail("Usuário não está associado a esta vaga");

                await _db.CandidateCollection.DeleteOneAsync(c =>
                    c.JobPositionId == positionId && c.UserId == userId,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
