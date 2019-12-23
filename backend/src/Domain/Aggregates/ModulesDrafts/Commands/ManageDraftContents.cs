using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class ManageDraftContents
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractContent> Contents { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractContent
        {
            public string Id { get; set; }
            public string SubjectId { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public long Duration { get; set; }
            public ContentType Type { get; set; }
            public string[] ReferenceUrls { get; set; }
            public List<ContractConcepts> Concepts { get; set; }
            public string Value { get; set; }
            public int? numPages { get; set; }
        }

        public class ContractConcepts
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
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
                    module = DeleteNonExisting(module, request.Contents);

                foreach (var ccontent in request.Contents)
                {
                    var subjectId = ObjectId.Parse(ccontent.SubjectId);
                    var subject = module.Subjects.FirstOrDefault(x => x.Id == subjectId);
                    if (subject == null)
                        return Result.Fail<bool>("Assunto não Encontrado");

                    Content content = null;
                    if (string.IsNullOrEmpty(ccontent.Id))
                    {
                        var contentConcepts = ccontent.Concepts.Select(
                            x => new ConceptPosition(x.Name, x.Positions, x.Anchors)
                        ).ToList();

                        var contentResult = Content.Create(
                            ccontent.Title, ccontent.Excerpt, contentConcepts,
                            ccontent.ReferenceUrls, ccontent.Value,
                            ccontent.Type, ccontent.Duration, ccontent.numPages
                        );

                        if (contentResult.IsFailure)
                            return Result.Fail(contentResult.Error);

                        content = contentResult.Data;
                        content.UpdatedAt = DateTimeOffset.UtcNow;
                        content.ReferenceUrls = new List<string>(ccontent.ReferenceUrls);

                        subject.Contents.Add(content);
                    }
                    else
                    {
                        content = subject.Contents.FirstOrDefault(x => x.Id == ObjectId.Parse(ccontent.Id));
                        if (content == null)
                            return Result.Fail($"Conteúdo não encontrado ({ccontent.Id} - {ccontent.Title})");

                        content.Title = ccontent.Title;
                        content.Excerpt = ccontent.Excerpt;

                        content.Duration = ccontent.Duration;
                        content.Type = ccontent.Type;
                        content.ReferenceUrls = new List<string>(ccontent.ReferenceUrls);
                        content.Concepts = ccontent.Concepts
                            .Select(x => new ConceptPosition(x.Name, x.Positions, x.Anchors)).ToList();
                        content.Value = ccontent.Value;
                        content.NumPages = ccontent.numPages;
                    }
                }

                await _db.ModuleDraftCollection.ReplaceOneAsync(
                    t => t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                var  newDraftList = new List<ModuleDraft>
                {
                    module
                };
                
                var changeLog = AuditLog.Create(userId, module.Id, module.GetType().ToString(),
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Update, oldValues);

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                return Result.Ok();
            }

            private ModuleDraft DeleteNonExisting(ModuleDraft module, List<ContractContent> contents)
            {
                foreach (var subject in module.Subjects)
                {
                    var requestIds = from content in contents
                                     where !string.IsNullOrEmpty(content.Id)
                                     select ObjectId.Parse(content.Id);

                    subject.Contents = subject.Contents.Where(
                        x => requestIds.Contains(x.Id)
                    ).ToList();
                }

                return module;
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
