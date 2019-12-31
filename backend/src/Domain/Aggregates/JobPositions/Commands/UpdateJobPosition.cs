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
    public class UpdateJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string JobPositionId { get; set; }
            public string Title { get; set; }
            public DateTimeOffset DueTo { get; set; }
            public int Priority { get; set; }
            public Employment Employment { get; set; }
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
                if (request.UserRole != "Recruiter" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<Result>("Acesso Negado");

                var positionId = ObjectId.Parse(request.JobPositionId);

                var position = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.Id == positionId)
                    .FirstOrDefaultAsync();

                if (position == null)
                    return Result.Fail("Vaga não encontrada");

                position.Title = request.Title;
                position.DueTo = request.DueTo;
                position.Priority = (JobPositionPriorityEnum) request.Priority;
                position.Employment = request.Employment;

                await _db.JobPositionCollection.ReplaceOneAsync(
                    p => p.Id == positionId, position,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
