using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MailChimp.Net.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserProgressHistory.Queries
{
    public class GetAnswersQuery
    {
        public class Contract : CommandContract<Result<List<AnswerInfo>>>
        {
            public string RequestUserId { get; set; }
            public string UserId { get; set; }
            public string TrackId { get; set; }
            public string ModuleId { get; set; }
        }

        public class AnswerInfo
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId QuestionId { get; set; }
            public ObjectId AnswerId { get; set; }
            public ObjectId UserId { get; set; }
            public string ModuleName { get; set; }
            public string SubjectName { get; set; }
            public string UserName { get; set; }
            public string Question { get; set; }
            public string QuestionConcepts { get; set; }
            public string Answer { get; set; }
            public string AnswerWrongConcepts { get; set; }
            public string BusinessGroup { get; set; }
            public string BusinessUnit { get; set; }
            public string Segment { get; set; }
            public int? QuestionLevel { get; set; }
            public bool CorrectAnswer { get; set; }
            public string AnswerDate { get; set; }
            public int? TotalDbQuestionNumber { get; set; }
            public int? TotalConceptNumber { get; set; }
            public decimal? LevelPercent { get; set; }
            public int? TotalQuestionNumber { get; set; }
            public int? ModuleQuestionsLimit { get; set; }
            public int? MaxPoints { get; set; }
            public int? TotalAnswers { get; set; }
            public int? InitWindow { get; set; }
            public int? EndWindow { get; set; }
            public int? TotalAccountablePoints { get; set; }
            public bool? HasAnsweredAllLevelQuestionsCorrectly { get; set; }
            public int? TotalApplicablePoints { get; set; }
            public decimal? AbsoluteProgress { get; set; }
            public int? OriginalLevel { get; set; }
            public int? FinalLevel { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<AnswerInfo>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IConfiguration config)
            {
                _db = db;
            }

            public async Task<Result<List<AnswerInfo>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var answers = _db.UserSubjectProgressCollection.AsQueryable();
                    var questions = _db.QuestionCollection.AsQueryable();

                    var query =
                        from a in answers
                        select new
                        {
                            a.ModuleId,
                            a.SubjectId,
                            a.Answers,
                            a.Questions,
                            a.Level,
                            a.UserId
                        };

                    if (!string.IsNullOrEmpty(request.TrackId))
                    {
                        var trackId = ObjectId.Parse(request.TrackId);
                        var trackModules = (await _db.TrackCollection.AsQueryable()
                            .Where(x => x.Id == trackId)
                            .SelectMany(x => x.ModulesConfiguration)
                            .Select(x => x.ModuleId)
                            .ToListAsync(cancellationToken)).ToArray();

                        query = query.Where(x => trackModules.Contains(x.ModuleId));

                        //filtrar pelos usuários da trilha
                        if (string.IsNullOrEmpty(request.UserId))
                        {
                            var trackUsers = (await _db.UserCollection.AsQueryable()
                                .Where(x => x.TracksInfo.Any(y => y.Id == trackId))
                                .Select(x => x.Id)
                                .ToListAsync(cancellationToken)).ToArray();
                            query = query.Where(x => trackUsers.Contains(x.UserId));
                        }
                    }
                    if (!string.IsNullOrEmpty(request.UserId))
                    {
                        var userId = ObjectId.Parse(request.UserId);
                        query = query.Where(x => x.UserId == userId);
                    }

                    if (!string.IsNullOrEmpty(request.ModuleId))
                    {
                        var moduleId = ObjectId.Parse(request.ModuleId);
                        query = query.Where(x => x.ModuleId == moduleId);
                    }

                    var userListAnswers = await query.ToListAsync(cancellationToken);

                    var answeredQuestions = userListAnswers
                        .SelectMany(x => x.Answers)
                        .Select(x => x.QuestionId)
                        .Distinct()
                        .ToArray();

                    var answeredUsers = userListAnswers.Select(x => x.UserId).Distinct().ToArray();
                    var answeredModules = userListAnswers.Select(x => x.ModuleId).Distinct().ToArray();

                    var users = await _db.UserCollection.AsQueryable()
                        .Where(x => answeredUsers.Contains(x.Id))
                        .Select(x => new { x.Id, x.Name, x.BusinessGroup, x.BusinessUnit, x.Segment })
                        .ToListAsync(cancellationToken);

                    var modules = await _db.ModuleCollection.AsQueryable()
                        .Where(x => answeredModules.Contains(x.Id))
                        .Select(x => new
                        {
                            ModuleId = x.Id,
                            ModuleName = x.Title,
                            Subjects = x.Subjects.Select(s => new { s.Id, s.Title })
                        })
                        .ToListAsync(cancellationToken);


                    var dbQuestions = await questions
                        .Where(x => answeredQuestions.Contains(x.Id))
                        .Select(x => new
                        {
                            x.Id,
                            x.Text,
                            x.Answers,
                            x.Concepts
                        })
                        .ToListAsync(cancellationToken);

                    var answersInfos = new List<AnswerInfo>();
                    foreach (var userAnswers in userListAnswers)
                    {
                        var user = users.FirstOrDefault(x => x.Id == userAnswers.UserId);
                        var module = modules.FirstOrDefault(x => x.ModuleId == userAnswers.ModuleId);
                        var subject = module?.Subjects?.FirstOrDefault(x => x.Id == userAnswers.SubjectId);

                        foreach (var userAnswer in userAnswers.Answers)
                        {
                            var dbQ = dbQuestions.FirstOrDefault(x =>
                                x.Id == userAnswer.QuestionId);
                            var dbAns = dbQ?.Answers.FirstOrDefault(x => x.Id == userAnswer.AnswerId);

                            var questionConcepts = userAnswer.QuestionConcepts ?? (dbQ?.Concepts?.Count > 0 ?
                                dbQ?.Concepts?.Select(c => c.Name).Aggregate((f, s) => f + "," + s) : null);
                            var answerWrongConcepts = userAnswer.AnswerWrongConcepts ?? (
                                                      dbAns?.Concepts != null &&
                                                      dbAns.Concepts.Count > 0 &&
                                                      dbAns.Concepts.Any(c => !c.IsRight) ?
                                dbAns.Concepts.Where(c => !c.IsRight).Select(c => c.Concept)
                                    .Aggregate((f, s) => f + "," + s) : null);

                            answersInfos.Add(new AnswerInfo()
                            {
                                UserId = userAnswers.UserId,
                                ModuleId = userAnswers.ModuleId,
                                SubjectId = userAnswers.SubjectId,
                                QuestionId = userAnswer.QuestionId,
                                AnswerId = userAnswer.AnswerId,
                                ModuleName = module?.ModuleName,
                                SubjectName = subject?.Title,
                                UserName = user?.Name,
                                Question = userAnswer.QuestionText ?? dbQ?.Text,
                                QuestionConcepts = questionConcepts,
                                Answer = userAnswer.AnswerText ?? dbAns?.Description,
                                AnswerWrongConcepts = answerWrongConcepts,
                                BusinessGroup = user?.BusinessGroup?.Name,
                                BusinessUnit = user?.BusinessUnit?.Name,
                                Segment = user?.Segment?.Name,
                                QuestionLevel = userAnswer.Level,
                                CorrectAnswer = userAnswer.CorrectAnswer,
                                AnswerDate = userAnswer.AnswerDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                TotalDbQuestionNumber = userAnswer.TotalDbQuestionNumber,
                                TotalConceptNumber = userAnswer.TotalConceptNumber,
                                LevelPercent = userAnswer.LevelPercent,
                                TotalQuestionNumber = userAnswer.TotalQuestionNumber,
                                ModuleQuestionsLimit = userAnswer.ModuleQuestionsLimit,
                                MaxPoints = userAnswer.MaxPoints,
                                TotalAnswers = userAnswer.TotalAnswers,
                                InitWindow = userAnswer.InitWindow,
                                EndWindow = userAnswer.EndWindow,
                                TotalAccountablePoints = userAnswer.TotalAccountablePoints,
                                HasAnsweredAllLevelQuestionsCorrectly = userAnswer.HasAnsweredAllLevelQuestionsCorrectly,
                                TotalApplicablePoints = userAnswer.TotalApplicablePoints,
                                AbsoluteProgress = userAnswer.AbsoluteProgress,
                                OriginalLevel = userAnswer.OriginalLevel,
                                FinalLevel = userAnswer.FinalLevel
                            });

                        }
                    }

                    answersInfos = answersInfos.OrderBy(x => x.ModuleName)
                        .ThenBy(x => x.SubjectName)
                        .ThenBy(x => x.UserName)
                        .ThenBy(x => x.AnswerDate)
                        .ToList();

                    return Result.Ok(answersInfos);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

        }
    }
}
