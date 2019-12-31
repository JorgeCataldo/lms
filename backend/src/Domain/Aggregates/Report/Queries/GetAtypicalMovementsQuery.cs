using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MailChimp.Net.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Report.Queries
{
    public class GetAtypicalMovementsQuery
    {
        public class Contract : IRequest<Result<List<AnswerInfo>>>
        {
            public string TrackIds { get; set; }
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
            public string UserName { get; set; }
            public string Question { get; set; }
            public string PreviousQuestion { get; set; }
            public string Answer { get; set; }
            public bool CorrectAnswer { get; set; }
            public DateTimeOffset AnswerDate { get; set; }
            public string FormatedAnswerDate { get; set; }
            public DateTimeOffset PreviousAnswerDate { get; set; }
            public string FormatedPreviousAnswerDate { get; set; }
            public string AnswerTimeDifference { get; set; }
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
                    List<string> trackIdsList = new List<string>();
                    var answersInfos = new List<AnswerInfo>();
                    var atypicalAnswers = new List<AnswerInfo>();

                    if (!string.IsNullOrEmpty(request.TrackIds))
                    {
                        trackIdsList = request.TrackIds.Split(',').ToList();
                    }
                    else
                    {
                        return Result.Fail<List<AnswerInfo>>("É necessário selecionar pelo menos uma trilha.");
                    }

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

                    for (int i = 0; i < trackIdsList.Count; i++)
                    {
                        var trackId = ObjectId.Parse(trackIdsList[i]);
                        var trackModules = (await _db.TrackCollection.AsQueryable()
                            .Where(x => x.Id == trackId)
                            .SelectMany(x => x.ModulesConfiguration)
                            .Select(x => x.ModuleId)
                            .ToListAsync(cancellationToken)).ToArray();

                        query = query.Where(x => trackModules.Contains(x.ModuleId));


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
                            .Select(x => new { x.Id, x.Name })
                            .ToListAsync(cancellationToken);

                        var modules = await _db.ModuleCollection.AsQueryable()
                            .Where(x => answeredModules.Contains(x.Id))
                            .Select(x => new
                            {
                                ModuleId = x.Id,
                                ModuleName = x.Title
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

                        foreach (var userAnswers in userListAnswers)
                        {
                            var user = users.FirstOrDefault(x => x.Id == userAnswers.UserId);
                            var module = modules.FirstOrDefault(x => x.ModuleId == userAnswers.ModuleId);

                            foreach (var userAnswer in userAnswers.Answers)
                            {
                                var dbQ = dbQuestions.FirstOrDefault(x =>
                                    x.Id == userAnswer.QuestionId);
                                var dbAns = dbQ?.Answers.FirstOrDefault(x => x.Id == userAnswer.AnswerId);                                

                                answersInfos.Add(new AnswerInfo()
                                {
                                    UserId = userAnswers.UserId,
                                    ModuleId = userAnswers.ModuleId,
                                    SubjectId = userAnswers.SubjectId,
                                    QuestionId = userAnswer.QuestionId,
                                    AnswerId = userAnswer.AnswerId,
                                    ModuleName = module?.ModuleName,
                                    UserName = user?.Name,
                                    Question = userAnswer.QuestionText ?? dbQ?.Text,
                                    Answer = userAnswer.AnswerText ?? dbAns?.Description,
                                    CorrectAnswer = userAnswer.CorrectAnswer,
                                    AnswerDate = userAnswer.AnswerDate
                                });

                            }
                        }
                    }

                    //var atypicalAnswers = answersInfos.Zip(answersInfos,
                    //    (curr, prev) => new 
                    //    {
                    //        UserId = curr.UserId,
                    //        Question = curr.Question,
                    //        Answer = curr.Answer,
                    //        CurrentAnswerDate = curr.AnswerDate,
                    //        PreviousAnswerDate = prev.AnswerDate,
                    //        TimeDifference = curr.AnswerDate

                    //    });

                    answersInfos = answersInfos.OrderBy(x => x.ModuleName)
                        .ThenBy(x => x.UserName)
                        .ThenBy(x => x.AnswerDate)
                        .ToList();

                    for (int i = 1; i < answersInfos.Count; i++)
                    {
                        if(answersInfos[i].ModuleId == answersInfos[i - 1].ModuleId
                            && answersInfos[i].UserId == answersInfos[i - 1].UserId)
                        {
                            var diff = answersInfos[i].AnswerDate - answersInfos[i - 1].AnswerDate;
                            var miliseconds = diff.Milliseconds;
                            var seconds = diff.Seconds;
                            if (diff.TotalSeconds < 10)
                            {
                                answersInfos[i].AnswerTimeDifference = diff.ToString();
                                answersInfos[i].PreviousQuestion = answersInfos[i - 1].Question;
                                answersInfos[i].PreviousAnswerDate = answersInfos[i - 1].AnswerDate;
                                answersInfos[i].FormatedAnswerDate = answersInfos[i].AnswerDate.ToString("dd/MM/yyyy hh:mm:ss.ff");
                                answersInfos[i].FormatedPreviousAnswerDate = answersInfos[i - 1].AnswerDate.ToString("dd/MM/yyyy hh:mm:ss.ff");
                                answersInfos[i].Question = answersInfos[i].Question;
                                answersInfos[i].CorrectAnswer = answersInfos[i].CorrectAnswer;
                                atypicalAnswers.Add(answersInfos[i]);
                            }
                        }
                    }

                    return Result.Ok(atypicalAnswers);
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
