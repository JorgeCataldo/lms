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
namespace Domain.Aggregates.JobPosition.Queries
{
    public class GetJobPositions
    {
        public class Contract : CommandContract<Result<List<JobPositionItem>>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class JobPositionItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public DateTimeOffset DueTo { get; set; }
            public JobPositionPriorityEnum Priority { get; set; }
            public JobPositionStatusEnum Status { get; set; }
            public int? CandidatesCount { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<JobPositionItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<List<JobPositionItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Recruiter" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<List<JobPositionItem>>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var positions = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.CreatedBy == userId)
                    .Select(p =>
                        new JobPositionItem() {
                            Id = p.Id,
                            Title = p.Title,
                            DueTo = p.DueTo,
                            Priority = p.Priority,
                            Status = p.Status
                        }
                    )
                    .ToListAsync();

                var positionsIds = positions.Select(p => p.Id);

                var candidates = await _db.CandidateCollection.AsQueryable()
                    .Where(c => positionsIds.Contains(c.JobPositionId))
                    .ToListAsync();

                foreach (var position in positions)
                {
                    position.CandidatesCount = candidates.Count(c =>
                        c.JobPositionId == position.Id
                    );
                }

                return Result.Ok(positions);
            }
        }
    }
}
