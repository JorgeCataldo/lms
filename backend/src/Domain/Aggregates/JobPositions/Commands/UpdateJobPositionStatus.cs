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
    public class UpdateJobPositionStatus
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string JobPositionId { get; set; }
            public int Status { get; set; }
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
                if (request.UserRole != "Recruiter" && request.UserRole != "HumanResources")
                    return Result.Fail<Result>("Acesso Negado");

                var positionId = ObjectId.Parse(request.JobPositionId);

                var position = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.Id == positionId)
                    .FirstOrDefaultAsync();

                if (position == null)
                    return Result.Fail("Vaga não encontrada");
                
                position.Status = (JobPositionStatusEnum) request.Status;

                await _db.JobPositionCollection.ReplaceOneAsync(
                    p => p.Id == positionId, position,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
