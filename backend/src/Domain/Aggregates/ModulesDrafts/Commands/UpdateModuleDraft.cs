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
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;
using Tag = Domain.ValueObjects.Tag;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class UpdateModuleDraft
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public bool Published { get; set; }
            public int? Duration { get; set; }
            public List<string> Tags { get; set; }
            public string CertificateUrl { get; set; }
            public List<string> TutorsIds { get; set; }
            public List<string> ExtraInstructorIds { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public bool? CreateInEcommerce { get; set; }
            public long? EcommerceId { get; set; }
            public ModuleGradeTypeEnum ModuleGradeType { get; set; }
            public int? ValidFor { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IMediator mediator, IConfiguration configuration)
            {
                _db = db;
                _mediator = mediator;
                _configuration = configuration;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var modId = ObjectId.Parse(request.Id);

                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == modId || x.ModuleId == modId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);


                if (request.UserRole == "Author" && module.CreatedBy != userId)
                    return Result.Fail("Você não tem permissão para alterar o módulo selecionado.");

                Duration duration = null;
                if (request.Duration.HasValue)
                {
                    var durationResult = Duration.Create(request.Duration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail(durationResult.Error);

                    duration = durationResult.Data;
                }

                Duration videoDuration = null;
                if (request.VideoDuration.HasValue)
                {
                    var durationResult = Duration.Create(request.VideoDuration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail(durationResult.Error);

                    videoDuration = durationResult.Data;
                }

                if (request.TutorsIds == null)
                    request.TutorsIds = new List<string>();

                if (request.ExtraInstructorIds == null)
                    request.ExtraInstructorIds = new List<string>();

                if (module == null)
                {
                    var originalModule = await GetModule(request.Id);
                    if (originalModule == null)
                        return Result.Fail("Módulo não existe");

                    module = await CreateModuleDraft(userId, request, originalModule, 
                        duration, videoDuration, cancellationToken
                    );
                }
                else
                {
                    var oldValues = JsonConvert.SerializeObject(new List<ModuleDraft>
                    {
                        module
                    });

                    module.Title = request.Title;
                    module.Excerpt = request.Excerpt;
                    module.InstructorId = string.IsNullOrEmpty(request.InstructorId) ? ObjectId.Empty : ObjectId.Parse(request.InstructorId);
                    module.Instructor = request.Instructor;
                    module.InstructorMiniBio = request.InstructorMiniBio;
                    module.InstructorImageUrl = request.InstructorImageUrl;
                    module.ImageUrl = request.ImageUrl;
                    module.VideoUrl = request.VideoUrl;
                    module.VideoDuration = videoDuration;
                    module.Published = request.Published;
                    module.Duration = duration;
                    module.Tags = request.Tags.Select(
                        t => Tag.Create(t).Data
                    ).ToList();
                    module.CertificateUrl = request.CertificateUrl;
                    module.TutorsIds = request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList();
                    module.ExtraInstructorIds = request.ExtraInstructorIds.Select(x => ObjectId.Parse(x)).ToList();
                    module.StoreUrl = request.StoreUrl;
                    module.EcommerceUrl = request.EcommerceUrl;
                    module.CreateInEcommerce = request.CreateInEcommerce.HasValue ? request.CreateInEcommerce.Value : false;
                    module.EcommerceId = request.EcommerceId;
                    module.ModuleGradeType = request.ModuleGradeType;
                    module.ValidFor = request.ValidFor;

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

                }

                return Result.Ok();
            }

            private async Task<ModuleDraft> CreateModuleDraft(ObjectId userId, Contract request, Module originalModule,
                Duration duration, Duration videoDuration, CancellationToken token
            ) {
                var tags = new List<Tag>();
                foreach (var tagStr in request.Tags)
                {
                    var tag = Tag.Create(tagStr);
                    tags.Add(tag.Data);
                }

                var tutorsIds = request.TutorsIds != null ?
                    request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList() :
                    new List<ObjectId>();

                var instructorId = String.IsNullOrEmpty(request.InstructorId) ?
                    ObjectId.Empty : ObjectId.Parse(request.InstructorId);

                var extraInstructorIds = request.ExtraInstructorIds != null ?
                    request.ExtraInstructorIds.Select(x => ObjectId.Parse(x)).ToList() :
                    new List<ObjectId>();

                var moduleId = ObjectId.Parse(request.Id);

                var module = ModuleDraft.Create(
                    moduleId, request.Title, request.Excerpt, request.ImageUrl, request.Published,
                    instructorId, request.Instructor, request.InstructorMiniBio, request.InstructorImageUrl,
                    duration, videoDuration, request.VideoUrl, request.CertificateUrl, request.StoreUrl, request.EcommerceUrl,
                    request.CreateInEcommerce.HasValue ? request.CreateInEcommerce.Value : false,
                    tags, tutorsIds, extraInstructorIds, request.ModuleGradeType
                ).Data;

                module.Subjects = originalModule.Subjects;
                module.Requirements = originalModule.Requirements;
                module.SupportMaterials = originalModule.SupportMaterials;

                await _db.ModuleDraftCollection.InsertOneAsync(
                    module, cancellationToken: token
                );

                var creationLog = AuditLog.Create(userId, moduleId, module.GetType().ToString(),
                    JsonConvert.SerializeObject(module), EntityAction.Add);

                await _db.AuditLogCollection.InsertOneAsync(creationLog);

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
