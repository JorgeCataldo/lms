using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class ManageDraftSupportMaterialsCommand
    {
        public class Contract : CommandContract<Result>
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
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var modId = ObjectId.Parse(request.ModuleId);

                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == modId || x.ModuleId == modId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                {
                    var originalModule = await GetModule(request.ModuleId);
                    if (originalModule == null)
                        return Result.Fail("Módulo não existe");

                    module = await CreateModuleDraft(
                        request, originalModule, cancellationToken
                    );
                }

                var oldValues = JsonConvert.SerializeObject(new List<ModuleDraft>
                {
                    module
                });

                if (request.DeleteNonExistent)
                {
                    var existingIds = from o in request.SupportMaterials
                                      where !string.IsNullOrEmpty(o.Id)
                                      select ObjectId.Parse(o.Id);

                    var include = from c in module.SupportMaterials
                                  where existingIds.Contains(c.Id)
                                  select c;

                    module.SupportMaterials = include.ToList();
                }

                foreach (var requestObj in request.SupportMaterials)
                {
                    SupportMaterial newObject = null;
                    if (string.IsNullOrEmpty(requestObj.Id))
                    {
                        var result = Create(
                            requestObj.Title,
                            requestObj.Description,
                            requestObj.DownloadLink,
                            (SupportMaterialTypeEnum)requestObj.Type
                        );

                        if (result.IsFailure)
                            return Result.Fail(result.Error);

                        newObject = result.Data;
                        newObject.UpdatedAt = DateTimeOffset.UtcNow;
                        module.SupportMaterials.Add(newObject);
                    }
                    else
                    {
                        newObject = module.SupportMaterials.FirstOrDefault(x => x.Id == ObjectId.Parse(requestObj.Id));
                        if (newObject == null)
                            return Result.Fail($"Material de suporte não encontrado");

                        newObject.Title = requestObj.Title;
                        newObject.Description = requestObj.Description;
                        newObject.DownloadLink = requestObj.DownloadLink;
                    }
                }

                await _db.ModuleDraftCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                var newDraftList = new List<ModuleDraft>
                {
                    module
                };

                var changeLog = AuditLog.Create(userId, module.Id, module.GetType().ToString(),
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Update, oldValues);

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                return Result.Ok();
            }

            private async Task<ModuleDraft> CreateModuleDraft(
                Contract request, Module originalModule, CancellationToken token
            ) {

                var moduleId = ObjectId.Parse(request.ModuleId);

                var module = ModuleDraft.Create(
                    moduleId, originalModule.Title, originalModule.Excerpt, originalModule.ImageUrl, originalModule.Published,
                    originalModule.InstructorId, originalModule.Instructor, originalModule.InstructorMiniBio, originalModule.InstructorImageUrl,
                    originalModule.Duration, originalModule.VideoDuration, originalModule.VideoUrl,
                    originalModule.CertificateUrl, originalModule.StoreUrl, originalModule.EcommerceUrl, false,
                    originalModule.Tags, originalModule.TutorsIds, originalModule.ExtraInstructorIds
                ).Data;

                module.Subjects = originalModule.Subjects;
                module.Requirements = originalModule.Requirements;
                module.SupportMaterials = originalModule.SupportMaterials;

                await _db.ModuleDraftCollection.InsertOneAsync(
                    module, cancellationToken: token
                );

                var dbModuleQuestions = await _db.QuestionCollection.AsQueryable()
                    .Where(q => q.ModuleId == moduleId)
                    .ToListAsync();

                if (dbModuleQuestions.Count > 0)
                {
                    var draftQuestions = dbModuleQuestions.Select(q =>
                        QuestionDraft.Create(
                            q.Id, module.Id,
                            q.Text, q.Level, q.Duration,
                            q.Concepts, q.Answers,
                            q.ModuleId, q.SubjectId
                        ).Data
                    );

                    await _db.QuestionDraftCollection.InsertManyAsync(
                        draftQuestions, cancellationToken: token
                    );
                }

                return module;
            }

            private async Task<Module> GetModule(string moduleId)
            {
                var dbModuleId = ObjectId.Parse(moduleId);

                return await _db.ModuleCollection.AsQueryable()
                    .Where(mod => mod.Id == dbModuleId)
                    .FirstOrDefaultAsync();
            }
        }
    }
}