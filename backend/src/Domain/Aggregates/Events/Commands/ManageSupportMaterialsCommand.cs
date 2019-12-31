using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;
using SupportMaterial = Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Events.Commands
{
    public class ManageSupportMaterialsCommand
    {
        public class Contract : CommandContract<Result<List<ContractSupportMaterials>>>
        {
            public string EventId { get; set; }
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

        public class Handler : IRequestHandler<Contract, Result<List<ContractSupportMaterials>>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<List<ContractSupportMaterials>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<List<ContractSupportMaterials>>("Acesso Negado");

                var mId = ObjectId.Parse(request.EventId);
                var evt = await (await _db
                    .Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return Result.Fail<List<ContractSupportMaterials>>("Evento não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!evt.InstructorId.HasValue || evt.InstructorId.Value != userId)
                        return Result.Fail<List<ContractSupportMaterials>>("Acesso Negado");
                }

                evt.SupportMaterials = evt.SupportMaterials ?? new List<SupportMaterial>();

                if (request.DeleteNonExistent)
                {
                    var existingIds = from o in request.SupportMaterials
                        where !string.IsNullOrEmpty(o.Id)
                        select ObjectId.Parse(o.Id);
                    // deixando apenas os subjects que vieram na coleção
                    var include = from c in evt.SupportMaterials    
                        where existingIds.Contains(c.Id)    
                        select c;
                    evt.SupportMaterials = include.ToList();
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
                        {
                            return Result.Fail<List<ContractSupportMaterials>>(result.Error);
                        }

                        newObject = result.Data;
                        newObject.UpdatedAt = DateTimeOffset.UtcNow;
                        evt.SupportMaterials.Add(newObject);
                    }
                    else
                    {
                        newObject = evt.SupportMaterials.FirstOrDefault(x => x.Id == ObjectId.Parse(requestObj.Id));
                        if (newObject == null)
                        {
                            return Result.Fail<List<ContractSupportMaterials>>($"Material de suporte não encontrado ({requestObj.Id} - {requestObj.Title})");
                        }

                        newObject.Title = requestObj.Title;
                        newObject.Description = requestObj.Description;
                        newObject.DownloadLink = requestObj.DownloadLink;
                    }
                }

                await _db.EventCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );
                
                return Result.Ok(request.SupportMaterials);
            }
        }
    }
}