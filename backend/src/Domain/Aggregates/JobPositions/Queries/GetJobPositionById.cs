using System;
using System.Collections.Generic;
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
    public class GetJobPositionById
    {
        public class Contract : CommandContract<Result<JobPositionItem>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public string JobPositionId { get; set; }
        }

        public class JobPositionItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public DateTimeOffset DueTo { get; set; }
            public JobPositionPriorityEnum Priority { get; set; }
            public JobPositionStatusEnum Status { get; set; }
            public List<CandidateItem> Candidates { get; set; }
            public Employment Employment { get; set; }
        }

        public class CandidateItem
        {
            public ObjectId UserId { get; set; }
            public string UserName { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public ObjectId CreatedBy { get; set; }
            public bool? Approved { get; set; }
            public bool Accepted { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<JobPositionItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<JobPositionItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Recruiter" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<JobPositionItem>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var position = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.CreatedBy == userId && p.Id == positionId)
                    .Select(p =>
                        new JobPositionItem() {
                            Id = p.Id,
                            Title = p.Title,
                            DueTo = p.DueTo,
                            Priority = p.Priority,
                            Status = p.Status,
                            Employment = p.Employment
                        }
                    )
                    .FirstOrDefaultAsync();

                if (position == null)
                    return Result.Fail<JobPositionItem>("Vaga não encontrada");

                position.Candidates = await _db.CandidateCollection.AsQueryable()
                    .Where(c => c.JobPositionId == positionId)
                    .Select(c => 
                        new CandidateItem() {
                            UserId = c.UserId,
                            UserName = c.UserName,
                            CreatedAt = c.CreatedAt,
                            CreatedBy = c.CreatedBy,
                            Approved = c.Approved,
                            Accepted = c.Accepted
                        }
                    )
                    .ToListAsync();

                return Result.Ok(position);
            }
        }
    }
}
