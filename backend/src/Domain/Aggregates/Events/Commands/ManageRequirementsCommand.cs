using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ManageRequirementsCommand
    {
        
        public class Contract : CommandContract<Result<bool>>
        {
            public string EventId { get; set; }
            public List<ContractRequirements> Requirements { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractRequirements
        {
            public string ModuleId { get; set; }
            public bool Optional { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<bool>("Acesso Negado");

                var mId = ObjectId.Parse(request.EventId);
                var evt = await (await _db
                    .Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return Result.Fail<bool>("Evento não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!evt.InstructorId.HasValue || evt.InstructorId.Value != userId)
                        return Result.Fail<bool>("Acesso Negado");
                }

                var results = request.Requirements.Select(x => 
                    Requirement.Create(
                        ObjectId.Parse(x.ModuleId),
                        x.Optional,
                        x.Level,
                        x.Percentage,
                        ProgressType.ModuleProgress
                    )
                ).ToArray();

                var combinedResults = Result.Combine(results);
                if (combinedResults.IsFailure)
                    return Result.Fail<bool>(combinedResults.Error);

                evt.Requirements = results.Select(x => x.Data).ToList();

                await _db.EventCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(true);
            }
        }
    }
}
