using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Aggregates.Users;
using Domain.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;
using static Domain.Aggregates.ValuationTests.ValuationTestResponse;

namespace Domain.Aggregates.ValuationTests.Commands
{
    public class SaveValuationTestResponseCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public List<ValuationTestQuestionItem> TestQuestions { get; set; }
        }

        public class ValuationTestQuestionItem
        {
            public string Id { get; set; }
            public string Answer { get; set; }
            public string Title { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.Id))
                    return Result.Fail("Acesso Negado");

                var testId = ObjectId.Parse(request.Id);
                var userId = ObjectId.Parse(request.UserId);

                var test = await _db.ValuationTestCollection.AsQueryable()
                    .Where(t => t.Id == testId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (test == null)
                    return Result.Fail<Contract>("Teste não existe");

                var response = await _db.Database
                    .GetCollection<ValuationTestResponse>("ValuationTestResponses")
                    .AsQueryable()
                    .Where(r => r.CreatedBy == userId && r.TestId == testId)
                    .FirstOrDefaultAsync();

                var questions = await GetQuestions(test.Id);

                var answers = request.TestQuestions.Select(q => new ValuationTestAnswer {
                    QuestionId = ObjectId.Parse(q.Id),
                    Answer = q.Answer,
                    Question = q.Title,
                    Grade = GetAnswerGrade(questions, q),
                    Percentage = GetAnswerPercentage(questions, q),
                    Type = GetAnswerType(questions, q)
                }).ToList();
                
                if (response == null)
                {
                    var user = await GetUser(userId, cancellationToken);

                    var newResponse = Create(
                        testId, test.Title,
                        userId, user.Name, user.RegistrationId,
                        answers
                    ).Data;

                    await _db.ValuationTestResponseCollection.InsertOneAsync(
                        newResponse, cancellationToken: cancellationToken
                    );
                }
                else
                {
                    response.Answers = answers;
                    response.CreatedAt = DateTimeOffset.Now;

                    await _db.ValuationTestResponseCollection.ReplaceOneAsync(
                        r => r.Id == response.Id, response,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }

            private int? GetAnswerGrade(List<ValuationTestQuestion> questions, ValuationTestQuestionItem answer)
            {
                var questionId = ObjectId.Parse(answer.Id);

                var question = questions.First(q => q.Id == questionId);

                if (question.Type == ValuationTestQuestion.ValuationTestQuestionType.Discursive)
                    return null;

                var correctAnswers = question.Options.Where(o => o.Correct).Select(o => o.Text);

                return correctAnswers.Contains(answer.Answer) ? question.Percentage : 0;
            }

            private int? GetAnswerPercentage(List<ValuationTestQuestion> questions, ValuationTestQuestionItem answer)
            {
                var questionId = ObjectId.Parse(answer.Id);

                var question = questions.First(q => q.Id == questionId);

                return question.Percentage;
            }

            private ValuationTestQuestionType? GetAnswerType(List<ValuationTestQuestion> questions, ValuationTestQuestionItem answer)
            {
                var questionId = ObjectId.Parse(answer.Id);

                var question = questions.First(q => q.Id == questionId);

                return question.Type;
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                return await _db.UserCollection.AsQueryable()
                    .Where(t => t.Id == userId)
                    .FirstOrDefaultAsync(token);
            }

            private async Task<List<ValuationTestQuestion>> GetQuestions(ObjectId testId)
            {
                var questionsQuery = _db.ValuationTestQuestionCollection
                    .AsQueryable()
                    .Where(q => q.TestId == testId);

                return await (
                    (IAsyncCursorSource<ValuationTestQuestion>)questionsQuery
                ).ToListAsync();
            }
        }
    }
}
