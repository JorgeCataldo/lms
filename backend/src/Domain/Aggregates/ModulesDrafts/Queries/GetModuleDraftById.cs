using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.ModulesDrafts.Queries
{
    public class GetModuleDraftById
    {
        public class Contract : CommandContract<Result<ModuleItem>>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public bool Published { get; set; }
            public string Excerpt { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public ObjectId? InstructorId { get; set; }
            public string ImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? Duration { get; set; }
            public int? VideoDuration { get; set; }
            public string[] Tags { get; set; }
            public List<SupportMaterialItem> SupportMaterials { get; set; }
            public List<SubjectItem> Subjects { get; set; }
            public List<ContractRequirements> Requirements { get; set; }
            public string CertificateUrl { get; set; }
            public List<ObjectId> TutorsIds { get; set; }
            public List<TutorInfo> Tutors { get; set; }
            public List<ObjectId> ExtraInstructorIds { get; set; }
            public List<TutorInfo> ExtraInstructors { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
            public int? QuestionsLimit { get; set; }
            public long? EcommerceId { get; set; }
            public bool CreateInEcommerce { get; set; }
            public ModuleGradeTypeEnum ModuleGradeType { get; set; }
            public List<EcommerceModuleDraft> EcommerceProducts { get; set; }
            public List<ManageModuleWeights> ModuleWeights { get; set; }
            public int? ValidFor { get; set; }
        }

        public class ModuleNameItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
        }

        public class SupportMaterialItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public string ImageUrl { get; set; }
            public SupportMaterialTypeEnum? Type { get; set; }
        }

        public class SubjectItem
        {
            public ObjectId Id { get; set; }
            public string[] Concepts { get; set; }
            public List<ContentItem> Contents { get; set; }
            public List<ContractUserProgress> UserProgresses { get; set; }
            public bool HasQuestions { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
        }

        public class ContentItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public long? Duration { get; set; }
            public List<string> ReferenceUrls { get; set; }
            public ConceptItem[] Concepts { get; set; }
            public string Value { get; set; }
            public ContentType Type { get; set; }
            public int? NumPages { get; set; }
        }

        public class ConceptItem
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
        }

        public class ContractRequirements
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public bool Optional { get; set; }
            public int? Level { get; set; }
            public decimal? Percentage { get; set; }
            public ContractUserProgress RequirementValue { get; set; }
        }

        public class ContractUserProgress
        {
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class TutorInfo
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ModuleItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<ModuleItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<ModuleItem>("Acesso Negado");

                if (String.IsNullOrEmpty(request.Id))
                    return Result.Fail<ModuleItem>("Id do Módulo não Informado");

                var moduleId = ObjectId.Parse(request.Id);

                var qry = await _db.Database
                    .GetCollection<ModuleItem>("ModulesDrafts")
                    .FindAsync(x => x.Id == moduleId, cancellationToken: cancellationToken);

                var module = await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (module == null)
                    return Result.Fail<ModuleItem>("Módulo não existe");

                if (request.UserRole == "Student" && !module.Published)
                {
                    ObjectId userId = ObjectId.Parse(request.UserId);
                    bool isInstructor = module.InstructorId.HasValue && module.InstructorId.Value == userId;

                    if (!isInstructor)
                    {
                        User user = await GetUserById(userId, cancellationToken);

                        var RecommendedModules = new List<ObjectId>();
                        if (user.ModulesInfo != null)
                            RecommendedModules = user.ModulesInfo.Select(m => m.Id).ToList();

                        if (!RecommendedModules.Contains(module.Id))
                        {
                            bool hasAccessByTracks = await CheckModuleInTracks(
                                user.TracksInfo.Select(t => t.Id).ToList(),
                                moduleId, cancellationToken
                            );

                            if (!hasAccessByTracks)
                                return Result.Fail<ModuleItem>("Acesso Negado");
                        }
                    }
                }

                foreach (var req in module.Requirements)
                {
                    var reqModule = await _db.Database
                        .GetCollection<ModuleNameItem>("Modules")
                        .AsQueryable()
                        .Where(x => x.Id == req.ModuleId)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    req.Title = reqModule.Title;
                    req.Level = req.RequirementValue.Level;
                    req.Percentage = req.RequirementValue.Percentage;
                    req.RequirementValue = null;
                }

                module = await CheckQuestions(module, cancellationToken);
                module = await CheckTutors(module, cancellationToken);
                module = await CheckExtraInstructors(module, cancellationToken);

                return Result.Ok(module);
            }

            private async Task<ModuleItem> CheckQuestions(ModuleItem dbModule, CancellationToken token)
            {
                List<Level> currentLevels = Level.GetLevels().Data;
                foreach (var subject in dbModule.Subjects)
                {
                    subject.HasQuestions = true;
                    var levelList = await _db.QuestionCollection
                        .AsQueryable()
                        .Where(x => x.SubjectId == subject.Id)
                        .Select(x => x.Level)
                        .Distinct()
                        .ToListAsync(cancellationToken: token);

                    foreach (var t in currentLevels)
                    {
                        if (levelList.Contains(t.Id)) continue;

                        subject.HasQuestions = false;
                        break;
                    }
                }

                return dbModule;
            }

            private async Task<User> GetUserById(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(
                        x => x.Id == userId,
                        cancellationToken: token
                     );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<bool> CheckModuleInTracks(List<ObjectId> tracksIds, ObjectId moduleId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Track>("Tracks")
                    .FindAsync(x =>
                        tracksIds.Contains(x.Id) &&
                        x.ModulesConfiguration.Any(mod =>
                            mod.ModuleId == moduleId
                        ),
                        cancellationToken: token
                     );

                return await query.AnyAsync(cancellationToken: token);
            }

            private async Task<ModuleItem> CheckTutors(ModuleItem dbModule, CancellationToken token)
            {
                dbModule.TutorsIds = dbModule.TutorsIds ?? new List<ObjectId>();
                dbModule.Tutors = await _db.Database
                    .GetCollection<TutorInfo>("Users")
                    .AsQueryable()
                    .Where(x => dbModule.TutorsIds.Contains(x.Id))
                    .ToListAsync();

                return dbModule;
            }

            private async Task<ModuleItem> CheckExtraInstructors(ModuleItem dbModule, CancellationToken token)
            {
                dbModule.ExtraInstructorIds = dbModule.ExtraInstructorIds ?? new List<ObjectId>();
                dbModule.ExtraInstructors = await _db.Database
                    .GetCollection<TutorInfo>("Users")
                    .AsQueryable()
                    .Where(x => dbModule.ExtraInstructorIds.Contains(x.Id))
                    .ToListAsync();

                return dbModule;
            }
        }
    }
}
