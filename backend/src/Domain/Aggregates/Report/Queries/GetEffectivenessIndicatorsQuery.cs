using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Report.Queries
{
    public class GetEffectivenessIndicatorsQuery
    {
        public class Contract : IRequest<Result<List<ExportItem>>>
        {
            public string TrackIds { get; set; }
            public List<UsersToExport> Users { get; set; }
            public string UserRole { get; set; }
        }

        public class UsersToExport
        {
            public string Name { get; set; }
            public string UserId { get; set; }
        }

        public class ExportItem
        {
            public string UserName { get; set; }
            public string TrackName { get; set; }
            public List<ModuleInfo> ModulesInfos { get; set; }
        }

        public class UserItem
        {
            public ObjectId UserId { get; set; }
            public string Name { get; set; }
        }

        public class ModuleInfo
        {
            public ObjectId ModuleId { get; set; }
            public string ModuleName { get; set; }
            public BdqPerformance BdqPerformances { get; set; }
            public VideoConsummation VideoConsummation { get; set; }
            public ConceptPerformance ConceptPerformances { get; set; }
        }

        public class BdqPerformance
        {
            public int CorrectQuestions { get; set; }
            public int AnsweredQuestions { get; set; }
            public decimal Effectiveness { get; set; }
        }

        public class VideoConsummation
        {
            public string Started { get; set; }
            public string Finished { get; set; }
        }

        public class ConceptPerformance
        {
            public int AcquiredConcepts { get; set; }
            public int ModuleConcepts { get; set; }
            public decimal Effectiveness { get; set; }
        }
        public class TrackItem
        {
            public ObjectId TrackId { get; set; }
            public string TrackName { get; set; }
            public List<TrackModule> ModulesConfiguration { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string ModuleName { get; set; }
        }

        public class SubjectItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId ModuleId { get; set; }
            public List<UserAnswer> Answers { get; set; }
        }

        public class QuestionItem
        {
            public ObjectId QuestionId { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId ModuleId { get; set; }
            public List<Concept> Concepts { get; set; }
        }

        public class ActionItem
        {
            public ObjectId UserId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string ModuleId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ExportItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ExportItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Secretary" && request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<List<ExportItem>>("Acesso Negado");

                var selectedTrackIds = new List<string>();
                selectedTrackIds = request.TrackIds.Split(',').ToList();
                var exportList = new List<ExportItem>();

                try
                {
                    for (int i = 0; i < selectedTrackIds.Count; i++)
                    {
                        var selectedTrack = ObjectId.Parse(selectedTrackIds[i]);

                        var track = await _db.TrackCollection
                        .AsQueryable()
                        .Where(x => x.Id == selectedTrack)
                        .Select(x => new TrackItem
                        {
                            TrackId = x.Id,
                            TrackName = x.Title,
                            ModulesConfiguration = x.ModulesConfiguration
                        }).FirstOrDefaultAsync();

                        var trackModulesIds = track.ModulesConfiguration.Select(x => x.ModuleId).ToList();
                        var trackModulesObjectIds = track.ModulesConfiguration.Select(x => x.ModuleId.ToString()).ToList();

                        var userTrackIds = _db.UserCollection
                        .AsQueryable()
                        .Where(x => x.TracksInfo.Any(t => selectedTrackIds.Contains(t.Id.ToString())))
                        .Select(x => x.Id);

                        var dbUsers = await _db.UserCollection
                        .AsQueryable()
                        .Where(
                            x => x.TracksInfo != null &&
                            x.TracksInfo.Any(y => y.Id == selectedTrack)
                        )
                        .Select(x => new UserItem
                        {
                            UserId = x.Id,
                            Name = x.Name
                        })
                        .ToListAsync(cancellationToken: cancellationToken);

                        var userIds = dbUsers.Select(x => x.UserId).ToList();

                        var videoActions = _db.ActionCollection
                           .AsQueryable()
                           .Where(x => userIds.Contains(x.CreatedBy) &&
                                    x.Description == "content-video" &&
                                    trackModulesObjectIds.Contains(x.ModuleId))
                           .Select(x => new ActionItem
                           {
                               CreatedAt = x.CreatedAt,
                               ModuleId = x.ModuleId,
                               UserId = x.CreatedBy
                           }).ToList();

                        var subjects = _db.UserSubjectProgressCollection
                           .AsQueryable()
                           .Where(x => userIds.Contains(x.UserId) &&
                                    trackModulesIds.Contains(x.ModuleId))
                           .Select(x => new SubjectItem
                           {
                               UserId = x.UserId,
                               ModuleId = x.ModuleId,
                               Answers = x.Answers
                           }).ToList();

                        var modulesIds = subjects.Select(x => x.ModuleId).Distinct().ToList();

                        var modules = _db.ModuleCollection
                            .AsQueryable()
                            .Where(x => modulesIds.Contains(x.Id))
                            .Select(x => new ModuleItem
                            {
                                ModuleId = x.Id,
                                ModuleName = x.Title
                            }).ToList();

                        var questions = _db.QuestionCollection
                            .AsQueryable()
                            .Where(x => modulesIds.Contains(x.ModuleId))
                            .Select(x => new QuestionItem
                            {
                                QuestionId = x.Id,
                                ModuleId = x.ModuleId,
                                SubjectId = x.SubjectId,
                                Concepts = x.Concepts
                            }).ToList();

                        foreach (UserItem user in dbUsers)
                        {
                            var userId = user.UserId;
                            var exportItem = new ExportItem()
                            {
                                UserName = user.Name,
                                TrackName = track.TrackName,
                                ModulesInfos = new List<ModuleInfo>()
                            };
                            var userModulesIds = subjects.Where(x => x.UserId == userId).Select(x => x.ModuleId).Distinct();
                            var userModules = modules.Where(x => modulesIds.Contains(x.ModuleId));
                            foreach (ModuleItem userModule in userModules)
                            {
                                exportItem.ModulesInfos.Add(new ModuleInfo()
                                {
                                    ModuleId = userModule.ModuleId,
                                    ModuleName = userModule.ModuleName,
                                    BdqPerformances = GetBdqPerformance(userId, subjects, userModule),
                                    VideoConsummation = GetVideoConsummation(userId, videoActions, userModule),
                                    ConceptPerformances = GetConceptPerformance(userId, questions, subjects, userModule)
                                });
                            }
                            exportList.Add(exportItem);
                        }
                    }
                }
                catch(Exception ex)
                {
                    return Result.Fail<List<ExportItem>>("Acesso Negado");
                }

                

                return Result.Ok(exportList);
            }

            private BdqPerformance GetBdqPerformance(ObjectId userId, List<SubjectItem> subjects, ModuleItem module)
            {
                
                var moduleAnsweredQuestions = subjects.Where(x => x.UserId == userId && x.ModuleId == module.ModuleId);
                var userAnswers = new List<UserAnswer>();
                foreach (List<UserAnswer> answers in moduleAnsweredQuestions.Select(x => x.Answers))
                {
                    userAnswers.AddRange(answers);
                }
                var correctQuestions = userAnswers.Where(x => x.CorrectAnswer).Count();
                var answeredQuestions = userAnswers.Count();
                return new BdqPerformance
                {
                    CorrectQuestions = correctQuestions,
                    AnsweredQuestions = answeredQuestions,
                    Effectiveness = answeredQuestions == 0 || correctQuestions == 0 ? 0 : 
                        ((decimal)correctQuestions / (decimal)answeredQuestions) * 100
                };
            }

            private VideoConsummation GetVideoConsummation(ObjectId userId, List<ActionItem> actions, ModuleItem module)
            {
                var userActions = actions.Where(x => x.UserId == userId && ObjectId.Parse(x.ModuleId) == module.ModuleId).OrderBy(x => x.CreatedAt);
                var first = userActions.FirstOrDefault();
                var last = userActions.LastOrDefault();
                if (first == null || last == null)
                {
                    return new VideoConsummation();
                }
                var videoConsumation =  new VideoConsummation
                {
                    Started = first.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    Finished = last.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")
                };

                return videoConsumation;
            }

            private ConceptPerformance GetConceptPerformance(ObjectId userId, List<QuestionItem> questions, List<SubjectItem> subjects, ModuleItem module)
            {
                var moduleAnsweredQuestions = subjects.Where(x => x.UserId == userId && x.ModuleId == module.ModuleId);
                var userAnswers = new List<UserAnswer>();
                foreach (List<UserAnswer> answers in moduleAnsweredQuestions.Select(x => x.Answers))
                {
                    userAnswers.AddRange(answers);
                }

                var allQuestionsIds = userAnswers.Select(x => x.QuestionId);
                var allConcepts = new List<Concept>();
                foreach (List<Concept> concepts in questions.Where(x => allQuestionsIds.Contains(x.QuestionId)).Select(x => x.Concepts))
                {
                    allConcepts.AddRange(concepts);
                }
                var allConceptsCount = allConcepts.Distinct().Count();

                var userCorrectAnswersQuestionsIds = userAnswers.Where(x => x.CorrectAnswer).Select(x => x.QuestionId);
                var userConcepts = new List<Concept>();
                foreach (List<Concept> concepts in questions.Where(x => userCorrectAnswersQuestionsIds.Contains(x.QuestionId)).Select(x => x.Concepts))
                {
                    userConcepts.AddRange(concepts);
                }
                var userConceptsCount = userConcepts.Distinct().Count();

                return new ConceptPerformance
                {
                    AcquiredConcepts = userConceptsCount,
                    ModuleConcepts = allConceptsCount,
                    Effectiveness = userConceptsCount == 0 || allConceptsCount == 0 ? 0 :
                        ((decimal)userConceptsCount / (decimal)allConceptsCount) * 100
                };
            }
        }
    }
}
