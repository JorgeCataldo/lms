using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestQuestion;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class ManageProfileTestCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public List<ProfileTestQuestionItem> TestQuestions { get; set; }
            public bool IsDefault { get; set; }
            public string UserRole { get; set; }
        }

        public class ProfileTestQuestionItem
        {
            public string Id { get; set; }
            public string TestId { get; set; }
            public string Title { get; set; }
            public int Percentage { get; set; }
            public ProfileTestQuestionType Type { get; set; }
            public List<ProfileTestQuestionOption> Options { get; set; }
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
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                return String.IsNullOrEmpty(request.Id) ?
                    await CreateNewTest(request, cancellationToken) :
                    await UpdateTest(request, cancellationToken);
            }

            public async Task<Result> CreateNewTest(Contract request, CancellationToken token)
            {
                var newTest = ProfileTest.Create(
                    request.Title,
                    new List<ObjectId>(),
                    request.IsDefault
                ).Data;

                await _db.ProfileTestCollection.InsertOneAsync(
                    newTest, cancellationToken: token
                );

                request.Id = newTest.Id.ToString();

                newTest.Questions = await UpdateQuestions(request, token);

                await _db.ProfileTestCollection.ReplaceOneAsync(
                    t => t.Id == newTest.Id, newTest,
                    cancellationToken: token
                );

                return Result.Ok(request);
            }

            public async Task<Result> UpdateTest(Contract request, CancellationToken token)
            {
                var dbTeste = await _db.ProfileTestCollection.AsQueryable()
                        .Where(t => t.Id == ObjectId.Parse(request.Id))
                        .FirstOrDefaultAsync();

                if (dbTeste == null)
                    return Result.Fail<Contract>("Teste não existe");

                dbTeste.Title = request.Title;
                dbTeste.IsDefault = request.IsDefault;
                dbTeste.Questions = await UpdateQuestions(request, token);

                await _db.ProfileTestCollection.ReplaceOneAsync(
                    t => t.Id == dbTeste.Id, dbTeste,
                    cancellationToken: token
                );

                return Result.Ok(request);
            }

            public async Task<List<ObjectId>> UpdateQuestions(Contract request, CancellationToken token)
            {
                var questionIds = new List<ObjectId>();

                if (!String.IsNullOrEmpty(request.Id))
                {
                    var testId = ObjectId.Parse(request.Id);

                    var questionsQuery = _db.ProfileTestQuestionCollection
                        .AsQueryable()
                        .Where(t => t.TestId == testId);

                    var testQuestions = await (
                        (IAsyncCursorSource<ProfileTestQuestion>)questionsQuery
                    ).ToListAsync();

                    var currentQuestions = request.TestQuestions
                        .Where(tQ => !String.IsNullOrEmpty(tQ.Id));

                    var currentIds = currentQuestions
                        .Select(tQ => ObjectId.Parse(tQ.Id));

                    await _db.ProfileTestQuestionCollection.DeleteManyAsync(f =>
                        !currentIds.Contains(f.Id),
                        cancellationToken: token
                    );

                    foreach (var question in currentQuestions)
                    {
                        var questionId = ObjectId.Parse(question.Id);
                        var dbQuestion = testQuestions.FirstOrDefault(f =>
                            f.Id == questionId
                        );

                        if (dbQuestion != null)
                        {
                            dbQuestion.Title = question.Title;
                            dbQuestion.Type = question.Type;
                            dbQuestion.Options = question.Options;
                            dbQuestion.Percentage = question.Percentage;

                            await _db.ProfileTestQuestionCollection.ReplaceOneAsync(t =>
                                t.Id == dbQuestion.Id, dbQuestion,
                                cancellationToken: token
                            );
                        }

                        questionIds.Add(dbQuestion.Id);
                    }
                }

                var newTestQuestions = request.TestQuestions
                    .Where(tQ => String.IsNullOrEmpty(tQ.Id));

                foreach (var question in newTestQuestions)
                {
                    var newQuestion = Create(
                        ObjectId.Parse(request.Id),
                        question.Title,
                        question.Percentage,
                        question.Type,
                        question.Options
                    ).Data;

                    await _db.ProfileTestQuestionCollection.InsertOneAsync(
                        newQuestion,
                        cancellationToken: token
                    );

                    questionIds.Add(newQuestion.Id);
                }

                return questionIds;
            }
        }
    }
}
