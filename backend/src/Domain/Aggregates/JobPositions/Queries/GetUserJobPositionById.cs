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
    public class GetUserJobPositionById
    {
        public class Contract : CommandContract<Result<JobPositionItem>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public string JobPositionId { get; set; }
        }

        public class JobPositionItem
        {
            public ObjectId JobPositionId { get; set; }
            public string Title { get; set; }
            public Employment Employment { get; set; }
            public bool Applied { get; set; }
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
                if (request.UserRole != "Student" && request.UserRole != "Admin")
                    return Result.Fail<JobPositionItem>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var position = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.Id == positionId)
                    .Select(p =>
                        new JobPositionItem() {
                            JobPositionId = p.Id,
                            Title = p.Title,
                            Employment = p.Employment
                        }
                    )
                    .FirstOrDefaultAsync();

                if (position == null)
                    return Result.Fail<JobPositionItem>("Vaga não encontrada");

                var userApplications = await _db.CandidateCollection.AsQueryable()
                  .Where(c => c.UserId == userId)
                  .ToListAsync();

                position.Applied = userApplications.FirstOrDefault(x => x.JobPositionId == positionId) == null ? false : true;

                return Result.Ok(position);
            }
        }
    }
}
