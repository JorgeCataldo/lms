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
    public class AddModuleDraftCommand
    {
        public class Contract : CommandContract<Result<ModuleDraft>>
        {
            public string ModuleId { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public bool? Published { get; set; }
            public List<string> Tags { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public string CertificateUrl { get; set; }
            public List<string> TutorsIds { get; set; }
            public List<string> ExtraInstructorIds { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
            public bool? CreateInEcommerce { get; set; } = false;
            public ModuleGradeTypeEnum ModuleGradeType { get; set; }
            public int? ValidFor { get; set; } = 0;

            public Contract()
            {
                Tags = new List<string>();
            }
        }

        public class Handler : IRequestHandler<Contract, Result<ModuleDraft>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<ModuleDraft>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail<ModuleDraft>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
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

                Module dbModule = null;
                List<Question> dbModuleQuestions = new List<Question>();

                if (!String.IsNullOrEmpty(request.ModuleId))
                    dbModule = await GetModule(request.ModuleId);

                if (dbModule == null)
                {
                    dbModule = Module.Create(
                        request.Title,
                        request.Excerpt,
                        request.Instructor,
                        request.ImageUrl,
                        new List<ObjectId>(),
                        instructorId,
                        new List<SupportMaterial>(),
                        new List<Requirement>(),
                        tags,
                        request.Published ?? false,
                        null,
                        new List<Subject>(),
                        request.CertificateUrl,
                        tutorsIds,
                        extraInstructorIds,
                        request.StoreUrl,
                        request.EcommerceUrl,
                        request.ModuleGradeType,
                        request.ValidFor
                    ).Data;

                    await _db.ModuleCollection.InsertOneAsync(
                        dbModule, cancellationToken: cancellationToken
                    );
                }
                else
                    dbModuleQuestions = await GetModuleQuestions(request.ModuleId);

                var draft = ModuleDraft.Create(
                    dbModule.Id, request.Title, request.Excerpt, request.ImageUrl, request.Published ?? false,
                    instructorId, request.Instructor, request.InstructorMiniBio, request.InstructorImageUrl,
                    null, null, null, request.CertificateUrl, request.StoreUrl, request.EcommerceUrl, request.CreateInEcommerce ?? false,
                    tags, tutorsIds, extraInstructorIds, request.ModuleGradeType
                ).Data;

                var newDraftList = new List<ModuleDraft>
                {
                    draft
                };

                await _db.ModuleDraftCollection.InsertOneAsync(
                    draft, cancellationToken: cancellationToken
                );

                var creationLog = AuditLog.Create(userId, draft.Id, draft.GetType().ToString(),
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Add);

                await _db.AuditLogCollection.InsertOneAsync(creationLog);

                if (dbModuleQuestions.Count > 0)
                {
                    var draftQuestions = dbModuleQuestions.Select(q =>
                        QuestionDraft.Create(
                            q.Id, draft.Id,
                            q.Text, q.Level, q.Duration,
                            q.Concepts, q.Answers,
                            q.ModuleId, q.SubjectId
                        ).Data
                    );

                    await _db.QuestionDraftCollection.InsertManyAsync(
                        draftQuestions, cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(draft);
            }

            private async Task<Module> GetModule(string moduleId)
            {
                var dbModuleId = ObjectId.Parse(moduleId);

                return await _db.ModuleCollection.AsQueryable()
                    .Where(mod => mod.Id == dbModuleId)
                    .FirstOrDefaultAsync();
            }

            private async Task<List<Question>> GetModuleQuestions(string moduleId)
            {
                var dbModuleId = ObjectId.Parse(moduleId);

                return await _db.QuestionCollection.AsQueryable()
                    .Where(q => q.ModuleId == dbModuleId)
                    .ToListAsync();
            }
        }
    }
}
