using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.Users;
using Domain.Aggregates.ValuationTests;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Modules.Queries
{
    public class GetModuleSummaryByIdQuery
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
            public string Excerpt {get;set;}
            public string Instructor {get;set;}
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
            public int? QuestionsLimit { get; set; }
            public long? EcommerceId { get; set; }
            public decimal? Grade { get; set; }
            public TrackModule ModuleConfiguration { get; set; }
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
            public int? Level { get; set; }
            public decimal? Percentage { get; set; }
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
                Track track = null;
                TrackModule moduleConfiguration = null;
                ModuleItem module = null;
                decimal? evaluationGrade = 0;
                
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<ModuleItem>("Acesso Negado");

                if (String.IsNullOrEmpty(request.Id))
                    return Result.Fail<ModuleItem>("Id do Módulo não Informado");

                var moduleId = ObjectId.Parse(request.Id);
                ObjectId userId = ObjectId.Parse(request.UserId);

                var qry = await _db.Database
                    .GetCollection<ModuleItem>("Modules")
                    .FindAsync(x => x.Id == moduleId, cancellationToken: cancellationToken);

                module = await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (module == null)
                    return Result.Fail<ModuleItem>("Módulo não existe");

                if (request.UserRole == "Student" || request.UserRole == "Admin" && !module.Published)
                {
                    bool isInstructor = module.InstructorId.HasValue && module.InstructorId.Value == userId;

                    if (!isInstructor)
                    {
                        User user = await GetUserById(userId, cancellationToken);

                        var RecommendedModules = new List<ObjectId>();
                        if (user.ModulesInfo != null)
                            RecommendedModules = user.ModulesInfo.Select(m => m.Id).ToList();

                        if (!RecommendedModules.Contains(module.Id))
                        {
                            List<ObjectId> trackIds = new List<ObjectId>();

                            var progressTracksId = await _db.UserTrackProgressCollection
                                .AsQueryable()
                                .Where(x => x.UserId == userId)
                                .Select(x => x.TrackId)
                                .ToListAsync(cancellationToken);

                            trackIds.AddRange(progressTracksId);

                            if (user.TracksInfo != null)
                                trackIds.AddRange(user.TracksInfo.Select(t => t.Id).ToList());

                            trackIds = trackIds.Distinct().ToList();

                            bool hasAccessByTracks = await CheckModuleInTracks(trackIds, moduleId, cancellationToken);

                            if (hasAccessByTracks)
                            {
                                track = await GetTrackFromModule(trackIds, moduleId, cancellationToken);

                                moduleConfiguration = track.ModulesConfiguration.Where(x => x.ModuleId == moduleId).FirstOrDefault();                                   
                            }
                            else
                            {
                                return Result.Fail<ModuleItem>("Acesso Negado");
                            }
                        }
                    }
                }

                foreach (var req in module.Requirements)
                {
                    req.Title = (await (await _db
                            .Database
                            .GetCollection<ModuleNameItem>("Modules")
                            .FindAsync(x => x.Id == req.ModuleId, cancellationToken: cancellationToken))
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken)).Title;
                    req.Level = req.RequirementValue.Level;
                    req.Percentage = req.RequirementValue.Percentage;
                    req.RequirementValue = null;
                }

                module = await CheckQuestions(module, cancellationToken);
                module = await CheckTutors(module, cancellationToken);
                module = await CheckExtraInstructors(module, cancellationToken);

                //### Enviar tb a lista de trilhas e uma variável para usar data de corte ou não
                var moduleGrade = Module.GetModulesGrades(_db, new List<ObjectId> { userId }, new List<ObjectId> { moduleId }, moduleConfiguration == null ? null : moduleConfiguration.CutOffDate).Data.FirstOrDefault();
                if (moduleGrade != null && moduleGrade.UserGrades != null && moduleGrade.UserGrades.Count > 0)
                {
                    var userModuleGrade = moduleGrade.UserGrades.First();
                    module.Grade = userModuleGrade.Grade * 10;
                }

                var moduleEvaluationTestsIds = await _db.ValuationTestCollection
                .AsQueryable()
                .Where(x =>
                    x.CreatedBy == ObjectId.Parse(request.UserId) &&
                    x.TestModules.Any(y => y.Id == moduleId)
                )
                .Select(x => x.Id)
                .ToListAsync();

                if (moduleEvaluationTestsIds.Count() > 0 && moduleConfiguration != null)
                {
                    if(track != null)
                    {
                        module.ModuleConfiguration = moduleConfiguration;
                    }

                    var moduleEvaluationTests = await _db.ValuationTestResponseCollection
                    .AsQueryable()
                    .Where(x =>
                        moduleEvaluationTestsIds.Contains(x.TestId)
                    ).ToListAsync();
                        
                    for (int i = 0; i < moduleEvaluationTests.Count(); i++)
                    {
                        for (int k = 0; k < moduleEvaluationTests[i].Answers.Count; k++)
                        {
                            evaluationGrade += moduleEvaluationTests[i].Answers[k].Grade;
                        }
                    }

                    evaluationGrade = evaluationGrade / moduleEvaluationTestsIds.Count();

                    //Média aritmética se peso do BDQ e Prova forem zero
                    if (moduleConfiguration.BDQWeight == 0 && moduleConfiguration.EvaluationWeight == 0)
                    {
                        module.Grade = (module.Grade + evaluationGrade) / 2;
                    }
                    else
                    {
                        var BDQDividerWeight = moduleConfiguration.BDQWeight == 0 ? 0 : moduleConfiguration.BDQWeight;
                        var BDQDividendWeight = moduleConfiguration.BDQWeight == 0 ? 1 : moduleConfiguration.BDQWeight;
                        var EvaluationDividerWeight = moduleConfiguration.EvaluationWeight == 0 ? 0 : moduleConfiguration.EvaluationWeight;
                        var EvaluationDividendWeight = moduleConfiguration.EvaluationWeight == 0 ? 1 : moduleConfiguration.EvaluationWeight;

                        module.Grade = (module.Grade * BDQDividendWeight + evaluationGrade * EvaluationDividendWeight)
                            / (BDQDividerWeight + EvaluationDividerWeight);
                    }
                }

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
                        .Where(x=>x.SubjectId == subject.Id)
                        .Select(x=>x.Level)
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

            private async Task<Track> GetTrackFromModule(List<ObjectId> tracksIds, ObjectId moduleId, CancellationToken token)
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

                return await query.FirstOrDefaultAsync(cancellationToken: token);
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
