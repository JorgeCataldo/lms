﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Enumerations;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTest;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;

namespace Domain.Aggregates.ValuationTests.Commands
{
    public class ManageValuationTestCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public ValuationTestTypeEnum Type { get; set; }
            public string Title { get; set; }
            public List<ValuationTestQuestionItem> TestQuestions { get; set; }
            public List<TestModuleItem> TestModules { get; set; }
            public List<TestTrackItem> TestTracks { get; set; }
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class ValuationTestQuestionItem
        {
            public string Id { get; set; }
            public string TestId { get; set; }
            public string Title { get; set; }
            public int Percentage { get; set; }
            public ValuationTestQuestionType Type { get; set; }
            public List<ValuationTestQuestionOption> Options { get; set; }
        }

        public class TestModuleItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public int? Percent { get; set; }
            public ValuationTestModuleTypeEnum? Type { get; set; }
        }

        public class TestTrackItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public int? Percent { get; set; }
            public int? Order { get; set; }
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
                if (request.UserRole != "Admin" && request.UserRole != "BusinessManager" && request.UserRole != "Author")
                    return Result.Fail("Acesso Negado");

                return String.IsNullOrEmpty(request.Id) ?
                    await CreateNewTest(request, cancellationToken) :
                    await UpdateTest(request, cancellationToken);
            }

            public async Task<Result> CreateNewTest(Contract request, CancellationToken token)
            {
                var testModuleList = new List<TestModule>();
                var testTrackList = new List<TestTrack>();
                for (int i = 0; i < request.TestModules.Count; i++)
                {
                    var currentTestModule = request.TestModules[i];
                    testModuleList.Add(new TestModule()
                    {
                        Id = ObjectId.Parse(currentTestModule.Id),
                        Title = currentTestModule.Title,
                        Percent = currentTestModule.Percent,
                        Type = currentTestModule.Type
                    });
                }

                for (int i = 0; i < request.TestTracks.Count; i++)
                {
                    var currentTestTrack = request.TestTracks[i];
                    testTrackList.Add(new TestTrack()
                    {
                        Id = ObjectId.Parse(currentTestTrack.Id),
                        Title = currentTestTrack.Title,
                        Percent = currentTestTrack.Percent,
                        Order = currentTestTrack.Order
                    });
                }

                var userId = ObjectId.Parse(request.UserId);
                var newTest = ValuationTest.Create(
                    userId,
                    request.Title,
                    request.Type,
                    new List<ObjectId>(),
                    testModuleList,
                    testTrackList
                ).Data;

                await _db.ValuationTestCollection.InsertOneAsync(
                    newTest, cancellationToken: token
                );

                request.Id = newTest.Id.ToString();

                newTest.Questions = await UpdateQuestions(request, token);

                await _db.ValuationTestCollection.ReplaceOneAsync(
                    t => t.Id == newTest.Id, newTest,
                    cancellationToken: token
                );

                return Result.Ok(request);
            }

            public async Task<Result> UpdateTest(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);
                var dbTeste = await _db.ValuationTestCollection.AsQueryable()
                        .Where(t => t.Id == ObjectId.Parse(request.Id))
                        .FirstOrDefaultAsync();

                if (dbTeste == null)
                    return Result.Fail<Contract>("Teste não existe");


                if ((request.UserRole == "Author" || request.UserRole == "Admin") && dbTeste.CreatedBy != userId)
                    return Result.Fail<Contract>("Você não tem permissão para alterar o módulo selecionado.");

                var testModuleList = new List<TestModule>();
                var testTrackList = new List<TestTrack>();
                for (int i = 0; i < request.TestModules.Count; i++)
                {
                    var currentTestModule = request.TestModules[i];
                    testModuleList.Add(new TestModule()
                    {
                        Id = ObjectId.Parse(currentTestModule.Id),
                        Title = currentTestModule.Title,
                        Percent = currentTestModule.Percent,
                        Type = currentTestModule.Type
                    });
                }

                for (int i = 0; i < request.TestTracks.Count; i++)
                {
                    var currentTestTrack = request.TestTracks[i];
                    testTrackList.Add(new TestTrack()
                    {
                        Id = ObjectId.Parse(currentTestTrack.Id),
                        Title = currentTestTrack.Title,
                        Percent = currentTestTrack.Percent,
                        Order = currentTestTrack.Order
                    });
                }

                dbTeste.Title = request.Title;
                dbTeste.Type = request.Type;
                dbTeste.TestModules = testModuleList;
                dbTeste.TestTracks = testTrackList;
                dbTeste.Questions = await UpdateQuestions(request, token);

                await _db.ValuationTestCollection.ReplaceOneAsync(
                    t => t.Id == dbTeste.Id && dbTeste.CreatedBy == userId, dbTeste,
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

                    var questionsQuery = _db.ValuationTestQuestionCollection
                        .AsQueryable()
                        .Where(t => t.TestId == testId);

                    var testQuestions = await (
                        (IAsyncCursorSource<ValuationTestQuestion>)questionsQuery
                    ).ToListAsync();

                    var currentQuestions = request.TestQuestions
                        .Where(tQ => !String.IsNullOrEmpty(tQ.Id));

                    /*var currentIds = currentQuestions
                        .Select(tQ => ObjectId.Parse(tQ.Id));

                    await _db.ValuationTestQuestionCollection.DeleteManyAsync(f =>
                        !currentIds.Contains(f.Id),
                        cancellationToken: token
                    );*/

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

                            await _db.ValuationTestQuestionCollection.ReplaceOneAsync(t =>
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

                    await _db.ValuationTestQuestionCollection.InsertOneAsync(
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
