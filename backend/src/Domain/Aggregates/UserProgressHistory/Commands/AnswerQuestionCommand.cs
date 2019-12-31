using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Questions;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
//ROLLBACK BDQ
namespace Domain.Aggregates.UserProgressHistory.Commands
{
    public class AnswerQuestionCommand
    {
        public class Contract : CommandContract<Result<UserAnswerInfo>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
            public string ModuleName { get; set; }
            public string SubjectId { get; set; }
            public string QuestionId { get; set; }
            public string AnswerId { get; set; }
            public List<string> Concepts { get; set; }
        }

        public class UserAnswerInfo
        {
            public bool HasAnsweredCorrectly { get; set; }
            public List<AnswerConcept> Concepts { get; set; }
            public bool HasAchievedNewLevel { get; set; }
            public int LevelAchieved { get; set; }
            public StartExamCommand.QuestionInfo NextQuestion { get; set; }
            public decimal Progress { get; set; }
            //public decimal PassPercentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<UserAnswerInfo>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<UserAnswerInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var subjectId = ObjectId.Parse(request.SubjectId);
                    var userId = ObjectId.Parse(request.UserId);
                    var questionId = ObjectId.Parse(request.QuestionId);
                    var answerId = ObjectId.Parse(request.AnswerId);
                    var progress = await _db
                        .UserSubjectProgressCollection
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ModuleId == moduleId &&
                                             x.UserId == userId &&
                                             x.SubjectId == subjectId, cancellationToken: cancellationToken);

                    if (progress == null)
                        return Result.Fail<UserAnswerInfo>("Por favor reinicie a avaliação.");

                    if (progress.Level == 4)
                        return Result.Fail<UserAnswerInfo>("Você já alcançou o nível máximo.");

                    var question = progress.Questions.FirstOrDefault(x => x.QuestionId == questionId);

                    if (question == null)
                        return Result.Fail<UserAnswerInfo>("Questão não encontrada. Por favor reinicie a avaliação.");

                    if (question.HasAnsweredCorrectly)
                        return Result.Fail<UserAnswerInfo>("Questão já respondida corretamente.");

                    question.HasAnsweredCorrectly = question.CorrectAnswerId == answerId;
                    question.Answered = true;
                    question.AnsweredCount += 1;

                    var dbQuestion = await _db.QuestionCollection.AsQueryable().FirstAsync(x => x.Id == questionId, cancellationToken: cancellationToken);

                    var applicableQuestionLevel = progress.Level == 3 ? 2 : progress.Level;

                    var answer = dbQuestion.Answers.First(x => x.Id == answerId);
                    var newAnswer = new UserAnswer()
                    {
                        AnswerDate = DateTimeOffset.UtcNow,
                        QuestionId = questionId,
                        AnswerPoints = answer.Points,
                        AnswerId = answerId,
                        CorrectAnswerId = question.CorrectAnswerId,
                        CorrectAnswer = question.HasAnsweredCorrectly,
                        QuestionText =
                            dbQuestion.Text.Length > 1000 ? dbQuestion.Text.Substring(1000) : dbQuestion.Text,
                        AnswerText = answer.Description,
                        Order = progress.Answers.Count,
                        Level = applicableQuestionLevel
                    };

                    progress.Answers.Add(newAnswer);

                    if (!question.HasAnsweredCorrectly &&
                        dbQuestion.Concepts != null &&
                        dbQuestion.Concepts.Count > 0)
                    {
                        newAnswer.QuestionConcepts =
                            dbQuestion.Concepts?.Select(c => c.Name).Aggregate((f, s) => f + "," + s);
                        if (answer.Concepts != null &&
                           answer.Concepts.Any(x => !x.IsRight))
                        {
                            request.Concepts = answer.Concepts.Where(x => !x.IsRight).Select(x => x.Concept)
                                .ToList();
                            newAnswer.AnswerWrongConcepts = request.Concepts.Aggregate((f, s) => f + "," + s);
                            await SaveStudentWrongConcepts(userId, request, cancellationToken);
                        }
                    }

                    /*
                     Validação de progresso
                     - Quantidade de respostas a serem validadas:
                     - Total = MIN(Tres perguntas * total de conceitos do assunto, Total de perguntas do BDQ, Limite de Perguntas do BDQ - se houver)
                     - Formula para passar de nivel:
                         passou = SUM(Pontuação das respostas dentro do total de perguntas válidas) / Pontuação Máxima (Total * 2) > Média necessária do nivel
                     - A média volante será o "Total", ou seja, a ordem de respostas certas e erradas deverá ser guardada, mas só vai contar as últimas "Total"                       
                     */


                    newAnswer.TotalDbQuestionNumber = await _db.QuestionCollection.AsQueryable().CountAsync(x =>
                        x.ModuleId == moduleId &&
                        x.SubjectId == subjectId &&
                        x.Level == applicableQuestionLevel, cancellationToken: cancellationToken);
                    var dbSubject = await _db.ModuleCollection.AsQueryable()
                        .Where(x => x.Id == moduleId)
                        .Select(x => x.Subjects.First(s => s.Id == subjectId))
                        .FirstAsync(cancellationToken);

                    newAnswer.TotalConceptNumber = dbSubject.Concepts.Count;
                    newAnswer.LevelPercent = progress.Level == 3
                        ? 1M
                        : dbSubject.UserProgresses.First(x => x.Level == progress.Level).Percentage;

                    newAnswer.TotalQuestionNumber = newAnswer.TotalDbQuestionNumber < newAnswer.TotalConceptNumber * 3
                        ? newAnswer.TotalDbQuestionNumber
                        : newAnswer.TotalConceptNumber * 3;

                    var module = await _db.ModuleCollection.AsQueryable()
                        .Where(m => m.Id == moduleId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (module != null && module.QuestionsLimit.HasValue)
                    {
                        newAnswer.ModuleQuestionsLimit = module.QuestionsLimit;
                        newAnswer.TotalQuestionNumber = newAnswer.TotalQuestionNumber < module.QuestionsLimit.Value ?
                            newAnswer.TotalQuestionNumber :
                            module.QuestionsLimit.Value;
                    }

                    newAnswer.MaxPoints = newAnswer.TotalQuestionNumber * 2;

                    newAnswer.TotalAnswers = progress.Answers.Count(x => x.Level == applicableQuestionLevel);
                    newAnswer.InitWindow = newAnswer.TotalAnswers.Value - newAnswer.TotalQuestionNumber.Value > 0 ? newAnswer.TotalAnswers.Value - newAnswer.TotalQuestionNumber.Value : 0;
                    newAnswer.EndWindow = newAnswer.TotalQuestionNumber.Value > newAnswer.TotalAnswers.Value ? newAnswer.TotalAnswers.Value : newAnswer.TotalQuestionNumber.Value;
                    var accountableAnswers = progress.Answers
                        .Where(x => x.Level == applicableQuestionLevel)
                        .ToList()
                        .GetRange(newAnswer.InitWindow.Value, newAnswer.EndWindow.Value);

                    newAnswer.TotalAccountablePoints = accountableAnswers.Sum(x => x.AnswerPoints);
                    newAnswer.TotalAccountablePoints = newAnswer.TotalAccountablePoints < 0 ? 0 : newAnswer.TotalAccountablePoints;

                    // se já respondeu todas as perguntas corretamente e não tem mais como responder outras perguntas, segue a vida e passa o cara de nivel
                    newAnswer.HasAnsweredAllLevelQuestionsCorrectly = !progress.Questions.Any(x => x.HasAnsweredCorrectly == false &&
                                                                      x.Level == applicableQuestionLevel);
                    newAnswer.TotalApplicablePoints = !newAnswer.HasAnsweredAllLevelQuestionsCorrectly.Value ? newAnswer.TotalAccountablePoints : newAnswer.MaxPoints;

                    newAnswer.AbsoluteProgress = (decimal)newAnswer.TotalApplicablePoints / (decimal)newAnswer.MaxPoints;

                    newAnswer.OriginalLevel = progress.Level;

                    if (progress.Level < 2 && newAnswer.AbsoluteProgress >= newAnswer.LevelPercent) // Valida os niveis iniciante e intermediario
                    {
                        progress.Level++;
                        progress.Progress = 0;
                        //progress.PassPercentage = 1;
                    }
                    else if (progress.Level == 2 &&
                             newAnswer.AbsoluteProgress >= newAnswer.LevelPercent &&
                             newAnswer.AbsoluteProgress < 1) // Valida se continua na validação para expert
                    {
                        progress.Level++;
                        progress.Progress = newAnswer.AbsoluteProgress.Value;
                        //progress.PassPercentage = 1;
                    }
                    else if (progress.Level >= 2 &&
                             newAnswer.AbsoluteProgress >= 1) // Valida se foi direto para expert
                    {
                        progress.Level = 4;
                        progress.Progress = 1;
                        //progress.PassPercentage = 0;
                    }
                    else
                    {
                        //progress.Progress = newAnswer.AbsoluteProgress.Value;
                        //decimal notAnswered = progress.Questions.Count(x => x.Level == applicableQuestionLevel && x.AnsweredCount == 0) * 2;
                        //decimal notAnsweredCorrectly = progress.Questions.Count(x => x.Level == applicableQuestionLevel && !x.HasAnsweredCorrectly && x.AnsweredCount == 1);
                        //if (notAnswered == 0 && notAnsweredCorrectly == 0)
                        //{
                        //    progress.PassPercentage = 0;
                        //}
                        //else
                        //{
                        //    var pass = (notAnswered + notAnsweredCorrectly) / (decimal)newAnswer.MaxPoints;
                        //    progress.PassPercentage = pass > 1 ? 1 : pass;
                        //}
                        progress.Progress = newAnswer.AbsoluteProgress.Value / (progress.Level < 3 ? newAnswer.LevelPercent.Value : 1);
                    }

                    newAnswer.FinalLevel = progress.Level;
                    // newAnswer.PassPercentage = progress.PassPercentage;

                    // progress.Points = (100 * progress.Level) + (progress.Level < 4 && newAnswer.FinalLevel == newAnswer.Level ? (int)(100 * (decimal)newAnswer.AbsoluteProgress) : 0);
                    progress.Points = (100 * progress.Level) + (progress.Level < 4 ? (int)(100 * progress.Progress) : 0);

                    await _db.UserSubjectProgressCollection.ReplaceOneAsync(x => x.Id == progress.Id, progress,
                        cancellationToken: cancellationToken);

                    var result = new UserAnswerInfo()
                    {
                        HasAnsweredCorrectly = question.HasAnsweredCorrectly,
                        HasAchievedNewLevel = newAnswer.OriginalLevel < progress.Level,
                        LevelAchieved = progress.Level,
                        Concepts = dbQuestion.Answers.First(x => x.Id == answerId).Concepts,
                        // PassPercentage = progress.PassPercentage,
                        Progress = progress.Progress
                    };
                    // Passou de nível
                    if (result.HasAchievedNewLevel)
                    {
                        // Atualizar Modulo
                        var userModuleProgress = await _db.UserModuleProgressCollection
                            .AsQueryable()
                            .FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.UserId == userId,
                                cancellationToken: cancellationToken);

                        var create = userModuleProgress == null;
                        if (create)
                            userModuleProgress = UserModuleProgress.Create(moduleId, userId);

                        var subjects = await _db.ModuleCollection
                            .AsQueryable()
                            .Where(x => x.Id == moduleId)
                            .Select(x => x.Subjects)
                            .FirstAsync(cancellationToken: cancellationToken);
                        var subjectProgress = await _db.UserSubjectProgressCollection
                            .AsQueryable()
                            .Where(x => x.ModuleId == moduleId && x.UserId == userId)
                            .ToListAsync(cancellationToken);

                        var ids = subjects.Select(x => x.Id);
                        subjectProgress = subjectProgress.Where(x => ids.Contains(x.SubjectId)).ToList();

                        var minLevel = subjectProgress.Count == subjects.Count
                            ? subjectProgress.Select(x => x.Level).Min()
                            : 0;
                        var nextLevelProgress = subjectProgress.Count(x => x.Level <= minLevel) + (subjects.Count - subjectProgress.Count);
                        userModuleProgress.Level = minLevel;
                        userModuleProgress.Progress = (decimal)(subjects.Count - nextLevelProgress) / (decimal)subjects.Count;

                        userModuleProgress.Points = subjectProgress.Sum(x => x.Points);

                        if (userModuleProgress.Level == 4)
                        {
                            userModuleProgress.CompletedAt = DateTimeOffset.Now;
                        }

                        if (create)
                        {
                            await _db.UserModuleProgressCollection.InsertOneAsync(userModuleProgress,
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await _db.UserModuleProgressCollection.ReplaceOneAsync(x => x.Id == userModuleProgress.Id,
                                userModuleProgress, cancellationToken: cancellationToken);
                        }

                        // Atualizar trilha
                        var userTracksProgress = await _db.UserTrackProgressCollection
                            .AsQueryable()
                            .Where(x => x.UserId == userId)
                            .ToListAsync(cancellationToken); ;
                        var tracks = await _db.TrackCollection.AsQueryable()
                            .ToListAsync(cancellationToken); ;


                        var userModulesProgress = await _db.UserModuleProgressCollection
                            .AsQueryable()
                            .Where(x => x.UserId == userId)
                            .Select(x => new { x.ModuleId, x.Level })
                            .ToListAsync(cancellationToken);

                        foreach (var track in tracks)
                        {
                            if (track.ModulesConfiguration.All(x => x.ModuleId != moduleId)) continue;

                            var usrProgress = userTracksProgress.FirstOrDefault(x => x.TrackId == track.Id);
                            var createTp = usrProgress == null;
                            if (createTp)
                                usrProgress = new UserTrackProgress(track.Id, userId, 0, 0);

                            var modulesCompleted = 0;
                            usrProgress.ModulesCompleted = new List<ObjectId>();

                            foreach (var cfg in track.ModulesConfiguration)
                            {
                                var mod = userModulesProgress.FirstOrDefault(x =>
                                    x.ModuleId == cfg.ModuleId && x.Level > cfg.Level);

                                if (mod != null)
                                {
                                    modulesCompleted++;
                                    usrProgress.ModulesCompleted.Add(mod.ModuleId);
                                }
                            }

                            usrProgress.Progress = (decimal)modulesCompleted / (decimal)track.ModulesConfiguration.Count;
                            if (usrProgress.Progress >= 1)
                            {
                                usrProgress.CompletedAt = DateTimeOffset.Now;
                            }
                            if (createTp)
                            {
                                await _db.UserTrackProgressCollection.InsertOneAsync(usrProgress,
                                    cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await _db.UserTrackProgressCollection.ReplaceOneAsync(
                                    x => x.Id == usrProgress.Id,
                                    usrProgress, cancellationToken: cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        // não passou de nível, 
                        var nextQuestionResult = await StartExamCommand.Handler.GetNextQuestion(progress, cancellationToken, _db);
                        if (nextQuestionResult.IsFailure)
                            return Result.Fail<UserAnswerInfo>(nextQuestionResult.Error);

                        result.NextQuestion = nextQuestionResult.Data;
                    }

                    return Result.Ok(result);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

            private async Task<bool> SaveStudentWrongConcepts(ObjectId userId, Contract request, CancellationToken token)
            {
                var wrongConcepts =
                    await _db.UserCollection.AsQueryable()
                            .Where(x => x.Id == userId)
                            .Select(x => x.WrongConcepts)
                            .FirstOrDefaultAsync(token) ?? new List<WrongConcept>();

                foreach (var concept in request.Concepts)
                {
                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var dbConcept = wrongConcepts.FirstOrDefault(x =>
                        x.Concept == concept &&
                        x.ModuleId == moduleId
                    );

                    if (dbConcept != null)
                        dbConcept.Count++;
                    else
                    {
                        wrongConcepts.Add(new WrongConcept
                        {
                            Concept = concept,
                            ModuleId = moduleId,
                            ModuleName = request.ModuleName,
                            Count = 1
                        });
                    }
                }

                var update = Builders<User>.Update.Set(usr => usr.WrongConcepts, wrongConcepts);
                await _db.UserCollection.UpdateOneAsync(x => x.Id == userId, update, cancellationToken: token);

                return true;
            }
        }
    }
}
