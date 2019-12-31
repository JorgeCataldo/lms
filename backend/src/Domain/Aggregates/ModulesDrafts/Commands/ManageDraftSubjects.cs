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
    public class ManageDraftSubjects
    {
        public class Contract : CommandContract<Result<List<ContractSubject>>>
        {
            public string ModuleId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractSubject> Subjects { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractSubject
        {
            public string Id { get; set; }
            public string[] Concepts { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public List<ContentItem> Contents { get; set; }
            public List<ContractUserProgress> UserProgresses { get; set; }
        }

        public class ContentItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public long Duration { get; set; }
            public List<string> ReferenceUrls { get; set; }
            public ConceptItem[] Concepts { get; set; }
            public string Value { get; set; }
            public ContentType Type { get; set; }
        }

        public class ConceptItem
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
        }

        public class ContractUserProgress
        {
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ContractSubject>>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<List<ContractSubject>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<List<ContractSubject>>("Acesso Negado");

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
                        return Result.Fail<List<ContractSubject>>("Módulo não existe");

                    module = await CreateModuleDraft(
                        request, originalModule, cancellationToken
                    );
                }

                if (request.Subjects != null)
                {
                    var oldValues = JsonConvert.SerializeObject(new List<ModuleDraft>
                    {
                        module
                    });

                    if (request.DeleteNonExistent)
                    {
                        var existingIds = from o in request.Subjects
                                          where !string.IsNullOrEmpty(o.Id)
                                          select ObjectId.Parse(o.Id);

                        var include = from c in module.Subjects
                                      where existingIds.Contains(c.Id)
                                      select c;

                        module.Subjects = include.ToList();
                    }

                    foreach (var csubject in request.Subjects)
                    {
                        Subject subject = null;
                        if (string.IsNullOrEmpty(csubject.Id))
                        {
                            var subjectResult = Subject.Create(csubject.Title, csubject.Excerpt, csubject.Concepts);
                            if (subjectResult.IsFailure)
                            {
                                return Result.Fail<List<ContractSubject>>(subjectResult.Error);
                            }

                            subject = subjectResult.Data;
                            subject.UpdatedAt = DateTimeOffset.UtcNow;
                            module.Subjects.Add(subject);
                        }
                        else
                        {
                            subject = module.Subjects.FirstOrDefault(x => x.Id == ObjectId.Parse(csubject.Id));
                            if (subject == null)
                            {
                                return Result.Fail<List<ContractSubject>>($"Assunto não encontrado ({csubject.Id} - {csubject.Title})");
                            }

                            subject.Title = csubject.Title;
                            subject.Excerpt = csubject.Excerpt;

                            var clist = Concept.GetConcepts(csubject.Concepts);
                            if (clist.IsFailure)
                                return Result.Fail<List<ContractSubject>>(clist.Error);

                            subject.Concepts = clist.Data;
                        }

                        if (csubject.UserProgresses != null)
                        {
                            subject.UserProgresses = new List<UserProgress>();

                            foreach (var crequirements in csubject.UserProgresses)
                            {
                                var req = UserProgress.Create(ProgressType.SubjectProgress, crequirements.Level,
                                    crequirements.Percentage);
                                if (req.IsFailure)
                                {
                                    return Result.Fail<List<ContractSubject>>(req.Error);
                                }

                                subject.UserProgresses.Add(req.Data);
                            }
                        }
                    }

                    var newDraftList = new List<ModuleDraft>
                    {
                        module
                    };

                    var changeLog = AuditLog.Create(userId, module.Id, module.GetType().ToString(),
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Update, oldValues);

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);
                }

                await _db.ModuleDraftCollection.ReplaceOneAsync(
                    t => t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                var response = module.Subjects.Select(subject =>
                    new ContractSubject()
                    {
                        Concepts = subject.Concepts.Select(c => c.Name).ToArray(),
                        Excerpt = subject.Excerpt,
                        Id = subject.Id.ToString(),
                        Title = subject.Title,
                        Contents = subject.Contents.Select(c => new ContentItem()
                        {
                            Duration = c.Duration,
                            Excerpt = c.Excerpt,
                            Id = c.Id,
                            ReferenceUrls = c.ReferenceUrls,
                            Title = c.Title,
                            Type = c.Type,
                            Value = c.Value,
                            Concepts = c.Concepts.Select(concept => new ConceptItem()
                            {
                                Anchors = concept.Anchors,
                                Positions = concept.Positions,
                                Name = concept.Name
                            }).ToArray(),
                        }).ToList(),
                        UserProgresses = subject.UserProgresses.Select(up => new ContractUserProgress
                        {
                            Level = up.Level,
                            Percentage = up.Percentage
                        }).ToList()
                    }
                ).ToList();

                return Result.Ok(response);
            }

            private async Task<ModuleDraft> CreateModuleDraft(
                Contract request, Module originalModule, CancellationToken token
            ) {
                var moduleId = ObjectId.Parse(request.ModuleId);

                var module = ModuleDraft.Create(
                    moduleId, originalModule.Title, originalModule.Excerpt, originalModule.ImageUrl, originalModule.Published,
                    originalModule.InstructorId, originalModule.Instructor, originalModule.InstructorMiniBio, originalModule.InstructorImageUrl,
                    originalModule.Duration, originalModule.VideoDuration, originalModule.VideoUrl,
                    originalModule.CertificateUrl, originalModule.StoreUrl, originalModule.EcommerceUrl, false, originalModule.Tags, 
                    originalModule.TutorsIds, originalModule.ExtraInstructorIds, originalModule.ModuleGradeType
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
