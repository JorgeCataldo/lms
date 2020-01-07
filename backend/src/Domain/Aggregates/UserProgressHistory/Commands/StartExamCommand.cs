using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Questions;
using Domain.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using Domain.Extensions;
using Domain.Base;
//ROLLBACK BDQ
namespace Domain.Aggregates.UserProgressHistory.Commands
{
    public class StartExamCommand
    {
        public class Contract : CommandContract<Result<QuestionInfo>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
        }

        public class QuestionInfo
        {
            public ObjectId Id { get; set; }
            public string Text { get; set; }
            public List<AnswerInfo> Answers { get; set; }
            public List<string> Concepts { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId ModuleId { get; set; }
        }

        public class AnswerInfo
        {
            public ObjectId Id { get; set; }
            public string Description { get; set; }
            public List<AnswerConcept> Concepts { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<QuestionInfo>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<QuestionInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var moduleId = ObjectId.Parse(request.ModuleId);
                    var subjectId = ObjectId.Parse(request.SubjectId);
                    var userId = ObjectId.Parse(request.UserId);
                    var progress = await _db
                        .UserSubjectProgressCollection
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ModuleId == moduleId &&
                                             x.UserId == userId &&
                                             x.SubjectId == subjectId, cancellationToken: cancellationToken);

                    if (progress != null)
                    {
                        //Já está no nivel máximo
                        if (progress.Level == 4 )
                            return Result.Fail<QuestionInfo>("Você já alcançou o nível máximo");

                        //Não tem mais perguntas disponiveis
                        //var applicableQuestionLevel = progress.Level == 3 ? 2 : progress.Level;
                        //var currentQuestions = progress.Questions.Where(x => x.Level == applicableQuestionLevel).ToList();
                        //if (currentQuestions.All(x => x.HasAnsweredCorrectly || x.AnsweredCount > 1))
                        //    return Result.Fail<QuestionInfo>("Não há a possibilidade de progredir no nivel atual. Você já alcançou o máximo de erros possíveis.");

                        //Buscado a nova lista de respostas
                        //var newQuestionList = await this.GetUserQuestionList(moduleId, subjectId, cancellationToken, progress);
                        var newQuestionList = await this.GetUserQuestionList(moduleId, subjectId, cancellationToken);
                        newQuestionList = newQuestionList.Shuffle();

                        // Retirando as perguntas não respondidas que não existem mais
                        var questionCount = progress.Questions.Count;
                        var newQuestionsIds = newQuestionList.Select(x => x.QuestionId);
                        progress.Questions = progress.Questions
                            .Where(x => x.HasAnsweredCorrectly || newQuestionsIds.Contains(x.QuestionId))
                            .OrderBy(x => x.Order).ToList();

                        if (questionCount != progress.Questions.Count || questionCount < newQuestionList.Count)
                        {
                            //Tirando as respostas existentes da nova lista
                            var questionIdList = progress.Questions.Select(x => x.QuestionId).ToList();

                            progress.Questions.AddRange(newQuestionList
                                .Where(x => !questionIdList.Contains(x.QuestionId)).OrderBy(x => x.Order));
                            //Ordenando depois de tudo pronto
                            for (var i = 0; i < progress.Questions.Count; i++)
                            {
                                progress.Questions[i].Order = i;
                            }

                            //Atualiza no banco de dados
                            await _db.UserSubjectProgressCollection.ReplaceOneAsync(t => t.Id == progress.Id, progress,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        progress = UserSubjectProgress.Create(moduleId, subjectId, userId);
                        //progress.Questions = await this.GetUserQuestionList(moduleId, subjectId, cancellationToken, null);
                        progress.Questions = await this.GetUserQuestionList(moduleId, subjectId, cancellationToken);

                        //Atualiza no banco de dados
                        await _db.UserSubjectProgressCollection.InsertOneAsync(progress,
                            cancellationToken: cancellationToken);
                    }

                    var nextQuestion = await GetNextQuestion(progress, cancellationToken, _db);

                    return nextQuestion;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

            public static async Task<Result<QuestionInfo>> GetNextQuestion(UserSubjectProgress progress, CancellationToken cancellationToken, IDbContext db)
            {
                UserQuestion nextQuestion = null;
                var applicableQuestionLevel = progress.Level == 3 ? 2 : progress.Level;
                var questions = progress.Questions.Where(x => x.Level == applicableQuestionLevel).ToList().Shuffle();
                //Pega a primeira pergunta não respondida
                foreach (var userQuestion in questions)
                {
                    if (userQuestion.Answered) continue;

                    nextQuestion = userQuestion;
                    break;
                }

                if (nextQuestion == null)
                {
                    //if (progress.Questions.All(x => x.Level != applicableQuestionLevel))
                    //    return Result.Fail<QuestionInfo>("Não foi possível obter nenhuma questão para o nível atual");

                    //Se não encontrou nenhuma é pq acabou o BDQ e tem que resetar as perguntas que ele respondeu errado
                    foreach (var userQuestion in progress.Questions.Where(x => x.Level == applicableQuestionLevel).ToList())
                    {
                        if (!userQuestion.Answered || userQuestion.HasAnsweredCorrectly) continue;
                        //if (!userQuestion.Answered || userQuestion.HasAnsweredCorrectly || userQuestion.AnsweredCount > 99) continue;

                        userQuestion.Answered = false;
                        if (nextQuestion == null)
                            nextQuestion = userQuestion;
                    }

                    //Atualiza no banco de dados
                    if (nextQuestion != null)
                        await db.UserSubjectProgressCollection.ReplaceOneAsync(t => t.Id == progress.Id, progress,
                            cancellationToken: cancellationToken);
                }

                if (nextQuestion == null)
                    return Result.Fail<QuestionInfo>("Não foi possível obter nenhuma questão para o nível atual");

                var question = await db.QuestionCollection
                    .AsQueryable()
                    .Where(x => x.Id == nextQuestion.QuestionId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                //validando se a configuração das perguntas mudou
                var dbcorrectAnswer = question.Answers.FirstOrDefault(x => x.Points == 2);
                if (dbcorrectAnswer == null)
                    return Result.Fail<QuestionInfo>("Pergunta com configuração errada. Por favor contate o suporte");
                if (nextQuestion.CorrectAnswerId != dbcorrectAnswer.Id)
                {
                    nextQuestion.CorrectAnswerId = dbcorrectAnswer.Id;
                    //Atualiza no banco de dados
                    await db.UserSubjectProgressCollection.ReplaceOneAsync(t => t.Id == progress.Id, progress,
                        cancellationToken: cancellationToken);
                }
                var rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Second);
                var response = new QuestionInfo()
                {
                    Text = question.Text,
                    Id = question.Id,
                    ModuleId = question.ModuleId,
                    SubjectId = question.SubjectId,
                    Answers = question.Answers.Select(a => new AnswerInfo()
                    {
                        Id = a.Id,
                        Description = a.Description,
                        Concepts = a.Concepts
                    }).ToList().Shuffle(),
                    Concepts = question.Concepts.Select(c => c.Name).ToList()
                };

                return Result.Ok(response);
            }

            private async Task<List<UserQuestion>> GetUserQuestionList(ObjectId moduleId, ObjectId subjectId, CancellationToken cancellationToken)
            {
                /*
                - Ordena as perguntas 
                   1 - pela qunatidade de conceitos abordados, do maior para o menor
                   2 - randomicamente (fica pra depois. Ver a necessidade)
                */
                var dbQuestions = await _db.QuestionCollection
                    .AsQueryable()
                    .Where(x => x.ModuleId == moduleId &&
                                x.SubjectId == subjectId)
                    .ToListAsync(cancellationToken);

                var badConfigCheck = dbQuestions.Count(x => x.Answers.All(q => q.Points != 2));
                if (badConfigCheck > 0)
                {
                    throw new Exception("Existem perguntas sem uma resposta certa");
                }

                var questions = dbQuestions
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Concepts.Count)
                    .Select(x => new UserQuestion()
                    {
                        Answered = false,
                        HasAnsweredCorrectly = false,
                        Level = x.Level,
                        QuestionId = x.Id,
                        // AnsweredCount = 0,
                        CorrectAnswerId = x.Answers.First(q => q.Points == 2).Id
                    })
                    .ToList();

                var i = 0;
                foreach (var userQuestion in questions)
                {
                    userQuestion.Order = i++;
                }

                return questions;
            }

            //private async Task<List<UserQuestion>> GetUserQuestionList(ObjectId moduleId, ObjectId subjectId, CancellationToken cancellationToken, UserSubjectProgress progress)
            //{
            //    /*
            //    - Ordena as perguntas 
            //       1 - pela qunatidade de conceitos abordados, do maior para o menor
            //       2 - randomicamente (fica pra depois. Ver a necessidade)
            //    */
            //    var dbQuestions = await _db.QuestionCollection
            //        .AsQueryable()
            //        .Where(x => x.ModuleId == moduleId)
            //        .ToListAsync(cancellationToken);

            //    var badConfigCheck = dbQuestions.Count(x => x.Answers.All(q => q.Points != 2));
            //    if (badConfigCheck > 0)
            //    {
            //        throw new Exception("Existem perguntas sem uma resposta certa");
            //    }

            //    var currentLevel = progress != null ? progress.Level : 0;
            //    var currentLevelQuestions = dbQuestions.Where(x => x.Level == currentLevel).ToList();

            //    var moduleSubjects = await _db.ModuleCollection
            //        .AsQueryable()
            //        .Where(x => x.Id == moduleId)
            //        .Select(x => x.Subjects)
            //        .FirstOrDefaultAsync(cancellationToken);

            //    var indexCurrentSubject = 0;

            //    for (int ind = 0; ind < moduleSubjects.Count; ind++)
            //    {
            //        if (moduleSubjects[ind].Id == subjectId)
            //        {
            //            indexCurrentSubject = ind;
            //            break;
            //        }
            //    }

            //    var questions = new List<UserQuestion>();
            //    var mixedListQuestion = new List<Question>();
            //    var currentSubjectQuestions = currentLevelQuestions.Where(x => x.SubjectId == subjectId).ToList().Shuffle();
            //    var otherSubjectsQuestions = dbQuestions.Where(x => x.SubjectId != subjectId).ToList().Shuffle();
            //    var windowQuestions = currentSubjectQuestions.Count;
            //    var currentSubjectInQuestionsQt = (int)Math.Round(windowQuestions * 0.8);
            //    var otherSubjectsInQuestionsQt = windowQuestions - currentSubjectInQuestionsQt;

            //    if (progress == null)
            //    {

            //        mixedListQuestion.AddRange(currentLevelQuestions.Take(currentSubjectInQuestionsQt));
            //        if (otherSubjectsQuestions.Count > otherSubjectsInQuestionsQt)
            //        {
            //            mixedListQuestion.AddRange(otherSubjectsQuestions.Take(otherSubjectsInQuestionsQt));
            //        }
            //        else if (otherSubjectsQuestions.Count > 0 && otherSubjectsQuestions.Count < otherSubjectsInQuestionsQt)
            //        {
            //            mixedListQuestion.AddRange(otherSubjectsQuestions);
            //            var windowLeft = windowQuestions - otherSubjectsQuestions.Count;
            //            mixedListQuestion.AddRange(currentLevelQuestions.TakeLast(windowLeft));
            //        }
            //        else
            //        {
            //            mixedListQuestion.AddRange(currentLevelQuestions.TakeLast(otherSubjectsInQuestionsQt));
            //        }

            //        questions = mixedListQuestion
            //        .Select(x => new UserQuestion()
            //        {
            //            Answered = false,
            //            HasAnsweredCorrectly = false,
            //            Level = x.Level,
            //            QuestionId = x.Id,
            //            AnsweredCount = 0,
            //            CorrectAnswerId = x.Answers.First(q => q.Points == 2).Id
            //        })
            //        .ToList();
            //    }
            //    else
            //    {
            //        var correctedAns = progress.Questions.Where(x => x.HasAnsweredCorrectly).ToList();
            //        var correctedAnsQIds = correctedAns.Select(x => x.QuestionId).ToList();
            //        currentSubjectQuestions = currentSubjectQuestions.Where(x => !correctedAnsQIds.Contains(x.Id)).ToList();

            //        if (currentSubjectQuestions.Count > currentSubjectInQuestionsQt)
            //        {
            //            mixedListQuestion.AddRange(currentSubjectQuestions.Take(currentSubjectInQuestionsQt));
            //        }
            //        else
            //        {
            //            mixedListQuestion.AddRange(currentSubjectQuestions);
            //        }

            //        var windowLeft = windowQuestions - mixedListQuestion.Count;

            //        if (windowLeft > otherSubjectsInQuestionsQt)
            //        {
            //            if (otherSubjectsQuestions.Count > 0)
            //            {
            //                if (otherSubjectsQuestions.Count > windowLeft)
            //                {
            //                    mixedListQuestion.AddRange(otherSubjectsQuestions.Take(windowLeft));
            //                }
            //                else
            //                {
            //                    mixedListQuestion.AddRange(otherSubjectsQuestions);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (otherSubjectsQuestions.Count > 0)
            //            {
            //                if (otherSubjectsQuestions.Count > windowLeft)
            //                {
            //                    mixedListQuestion.AddRange(otherSubjectsQuestions.Take(windowLeft));
            //                }
            //                else
            //                {
            //                    mixedListQuestion.AddRange(otherSubjectsQuestions);
            //                }
            //            }
            //        }

            //        questions = mixedListQuestion
            //        .Select(x => new UserQuestion()
            //        {
            //            Answered = false,
            //            HasAnsweredCorrectly = false,
            //            Level = x.Level,
            //            QuestionId = x.Id,
            //            AnsweredCount = 0,
            //            CorrectAnswerId = x.Answers.First(q => q.Points == 2).Id
            //        })
            //        .ToList();
            //    }

            //    var i = 0;
            //    foreach (var userQuestion in questions)
            //    {
            //        userQuestion.Order = i++;
            //    }

            //    return questions.Shuffle();
            //}
        }
    }
}
