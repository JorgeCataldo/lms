using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestResponse;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class SaveProfileTestResponseCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public List<ProfileTestQuestionItem> TestQuestions { get; set; }
        }

        public class ProfileTestQuestionItem
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

                var test = await _db.ProfileTestCollection.AsQueryable()
                    .Where(t => t.Id == testId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (test == null)
                    return Result.Fail<Contract>("Teste não existe");

                var response = await _db.Database
                    .GetCollection<ProfileTestResponse>("ProfileTestResponses")
                    .AsQueryable()
                    .Where(r => r.CreatedBy == userId && r.TestId == testId)
                    .FirstOrDefaultAsync();

                var questions = await GetQuestions(test.Id);

                var answers = request.TestQuestions.Select(q => new ProfileTestAnswer {
                    QuestionId = ObjectId.Parse(q.Id),
                    Answer = q.Answer,
                    Question = q.Title,
                    Grade = GetAnswerGrade(questions, q)
                }).ToList();

                if (response == null && !test.IsDefault)
                    return Result.Fail<Contract>("Acesso Negado");
                else if (response == null)
                {
                    var user = await GetUser(userId, cancellationToken);

                    var newResponse = Create(
                        testId, test.Title,
                        userId, user.Name, user.RegistrationId,
                        answers
                    ).Data;

                    await _db.ProfileTestResponseCollection.InsertOneAsync(
                        newResponse, cancellationToken: cancellationToken
                    );
                }
                else
                {
                    response.Answers = answers;
                    response.CreatedAt = DateTimeOffset.Now;

                    await _db.ProfileTestResponseCollection.ReplaceOneAsync(
                        r => r.Id == response.Id, response,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }

            private int? GetAnswerGrade(List<ProfileTestQuestion> questions, ProfileTestQuestionItem answer)
            {
                var questionId = ObjectId.Parse(answer.Id);

                var question = questions.First(q => q.Id == questionId);

                if (question.Type == ProfileTestQuestion.ProfileTestQuestionType.Discursive)
                    return null;

                var correctAnswers = question.Options.Where(o => o.Correct).Select(o => o.Text);

                return correctAnswers.Contains(answer.Answer) ? question.Percentage : 0;
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                return await _db.UserCollection.AsQueryable()
                    .Where(t => t.Id == userId)
                    .FirstOrDefaultAsync(token);
            }

            private async Task<List<ProfileTestQuestion>> GetQuestions(ObjectId testId)
            {
                var questionsQuery = _db.ProfileTestQuestionCollection
                    .AsQueryable()
                    .Where(q => q.TestId == testId);

                return await (
                    (IAsyncCursorSource<ProfileTestQuestion>)questionsQuery
                ).ToListAsync();
            }
        }
    }
}
