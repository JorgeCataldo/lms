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
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Modules.Commands
{
    public class ManageSupportMaterialsCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string ModuleId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractSupportMaterials> SupportMaterials { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractSupportMaterials
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public int Type { get; set; }
        }

        public class Handler : IRequestHandler<ManageSupportMaterialsCommand.Contract, Result<bool>>
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

                if (request.DeleteNonExistent)
                {
                    var existingIds = from o in request.SupportMaterials
                        where !string.IsNullOrEmpty(o.Id)
                        select ObjectId.Parse(o.Id);
                    // deixando apenas os subjects que vieram na coleção
                    var include = from c in module.SupportMaterials    
                        where existingIds.Contains(c.Id)    
                        select c;
                    module.SupportMaterials = include.ToList();
                }
                //Criando os assuntos
                foreach (var requestObj in request.SupportMaterials)
                {
                    SupportMaterial newObject = null;
                    if (string.IsNullOrEmpty(requestObj.Id))
                    {
                        var result = SupportMaterial.Create(
                            requestObj.Title,
                            requestObj.Description,
                            requestObj.DownloadLink,
                            (SupportMaterialTypeEnum)requestObj.Type
                        );

                        if (result.IsFailure)
                            return Result.Fail<bool>(result.Error);

                        newObject = result.Data;
                        newObject.UpdatedAt = DateTimeOffset.UtcNow;
                        module.SupportMaterials.Add(newObject);
                    }
                    else
                    {
                        newObject = module.SupportMaterials.FirstOrDefault(x => x.Id == ObjectId.Parse(requestObj.Id));
                        if (newObject == null)
                        {
                            return Result.Fail<bool>($"Material de suporte não encontrado ({requestObj.Id} - {requestObj.Title})");
                        }

                        newObject.Title = requestObj.Title;
                        newObject.Description = requestObj.Description;
                        newObject.DownloadLink = requestObj.DownloadLink;
                    }
                }

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(true);
            }
        }
    }
}