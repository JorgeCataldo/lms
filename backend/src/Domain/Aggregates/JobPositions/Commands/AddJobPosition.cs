using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;
namespace Domain.Aggregates.JobPosition.Commands
{
    public class AddJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
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

                var recruiterId = ObjectId.Parse(request.UserId);

                var position = JobPosition.Create(
                    recruiterId, request.Title, request.DueTo,
                    (JobPositionPriorityEnum) request.Priority,
                    request.Employment
                ).Data;

                await _db.JobPositionCollection.InsertOneAsync(
                    position, cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
