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
using Tg4.Infrastructure.Functional;
using Tag = Domain.ValueObjects.Tag;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class CloneModuleDraft
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
            public string UserRole { get; set; }
            public string CertificateUrl { get; set; }
            public List<string> TutorsIds { get; set; }
            public string StoreUrl { get; set; }
            public bool? CreateInEcommerce { get; set; } = false;
            public int? ValidFor { get; set; } = 0;
            public string UserId { get; set; }

            public Contract()
            {
                Tags = new List<string>();
            }
        }

        public class Handler : IRequestHandler<Contract, Result<ModuleDraft>>
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

            public async Task<Result<ModuleDraft>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.UserRole != "Admin" && request.UserRole != "Author")
                        return Result.Fail<ModuleDraft>("Acesso Negado");

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

                    Module dbModule = null;
                    Module dbModuleClone = null;
                    List<Question> dbModuleQuestions = new List<Question>();

                    if (!String.IsNullOrEmpty(request.ModuleId))
                        dbModule = await GetModule(request.ModuleId);

                    dbModuleClone = Module.Create(
                        request.Title,
                        dbModule.Excerpt,
                        dbModule.Instructor,
                        dbModule.ImageUrl,
                        new List<ObjectId>(),
                        instructorId,
                        new List<SupportMaterial>(),
                        new List<Requirement>(),
                        tags,
                        false,
                        null,
                        new List<Subject>(),
                        dbModule.CertificateUrl,
                        tutorsIds,
                        dbModule.ExtraInstructorIds,
                        dbModule.StoreUrl,
                        dbModule.EcommerceUrl,
                        dbModule.ModuleGradeType,
                        dbModule.ValidFor
                    ).Data;

                    await _db.ModuleCollection.InsertOneAsync(
                        dbModuleClone, cancellationToken: cancellationToken
                    );

                    dbModuleQuestions = await GetModuleQuestions(request.ModuleId);
                    
                    var draft = ModuleDraft.Create(
                        dbModuleClone.Id, dbModuleClone.Title, dbModuleClone.Excerpt, dbModuleClone.ImageUrl, dbModuleClone.Published,
                        instructorId, dbModuleClone.Instructor, dbModuleClone.InstructorMiniBio, dbModuleClone.InstructorImageUrl,
                        null, null, null, dbModuleClone.CertificateUrl, dbModuleClone.StoreUrl, dbModuleClone.EcommerceUrl, request.CreateInEcommerce ?? false,
                        tags, tutorsIds
                    ).Data;

                    draft.Subjects = dbModule.Subjects;
                    draft.Requirements = dbModule.Requirements;
                    draft.SupportMaterials = dbModule.SupportMaterials;

                    await _db.ModuleDraftCollection.InsertOneAsync(
                        draft, cancellationToken: cancellationToken
                    );

                    if (dbModuleQuestions.Count > 0)
                    {
                        var draftQuestions = dbModuleQuestions.Select(q =>
                            QuestionDraft.Create(
                                ObjectId.GenerateNewId(), draft.Id,
                                q.Text, q.Level, q.Duration,
                                q.Concepts, q.Answers,
                                dbModuleClone.Id, q.SubjectId
                            ).Data
                        );

                        await _db.QuestionDraftCollection.InsertManyAsync(
                            draftQuestions, cancellationToken: cancellationToken
                        );
                    }


                    return Result.Ok(draft);
                }
                catch(Exception ex)
                {
                    return Result.Fail<ModuleDraft>(ex.Message);
                }
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
