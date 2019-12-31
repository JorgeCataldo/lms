using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class SetQuestionsLimitCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
            public int? QuestionsLimit { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                var moduleId = ObjectId.Parse(request.ModuleId);
                var module = await _db.ModuleCollection
                    .AsQueryable()
                    .Where(x => x.Id == moduleId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (module == null)
                    return Result.Fail<bool>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if ((!Module.IsInstructor(module, userId).Data) && !module.TutorsIds.Contains(userId))
                        return Result.Fail<bool>("Acesso Negado");
                }

                module.QuestionsLimit = request.QuestionsLimit;

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
