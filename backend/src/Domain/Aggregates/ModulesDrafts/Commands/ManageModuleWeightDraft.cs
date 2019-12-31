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

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class ManageModuleWeightDraft
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string UserRole { get; set; }
            public string ModuleId { get; set; }
            public List<ModuleWeightItem> Weight { get; set; }
        }

        public class ModuleWeightItem
        {
            public string Content { get; set; }
            public int Weight { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<Contract>("Id do Curso não informado");

                var moduleId = ObjectId.Parse(request.ModuleId);
                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(t => t.Id == moduleId)
                    .FirstOrDefaultAsync();

                if (module == null)
                    return Result.Fail<Contract>("Curso não existe");

                var products = new List<ManageModuleWeights>();
                foreach (var product in request.Weight)
                {

                    var moduleWeight = ManageModuleWeights.Create(
                        product.Content,
                        product.Weight
                    );

                    products.Add(moduleWeight.Data);
                }

                module.ModuleWeights = products;

                await _db.ModuleDraftCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
