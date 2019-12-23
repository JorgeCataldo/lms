using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class ManageRequirementsCommand
    {
        
        public class Contract : CommandContract<Result<bool>>
        {
            public string ModuleId { get; set; }
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

        public class ContractUserProgress
        {
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
                if (request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                var mId = ObjectId.Parse(request.ModuleId);
                var module = await (await _db
                    .Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                    return Result.Fail<bool>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!Module.IsInstructor(module, userId).Data)
                        return Result.Fail<bool>("Acesso Negado");
                }

                var results = request.Requirements.Select(x => Requirement.Create(ObjectId.Parse(x.ModuleId),
                    x.Optional, x.Level, x.Percentage, ProgressType.ModuleProgress)).ToArray();

                var combinedResults = Result.Combine(results);
                if (combinedResults.IsFailure)
                    return Result.Fail<bool>(combinedResults.Error);

                module.Requirements = results.Select(x => x.Data).ToList();

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(true);
            }
        }
    }
}
