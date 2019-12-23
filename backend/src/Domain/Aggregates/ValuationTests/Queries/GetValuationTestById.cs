using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTest;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetValuationTestByIdQuery
    {
        public class Contract : CommandContract<Result<ValuationTestItem>>
        {
            public string TestId { get; set; }
            public string UserRole { get; set; }
        }

        public class ValuationTestItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ValuationTestQuestionItem> TestQuestions { get; set; }
            public List<TestModule> TestModules { get; set; }
            public List<TestTrack> TestTracks { get; set; }
        }

        public class ValuationTestQuestionItem
        {
            public ObjectId Id { get; set; }
            public ObjectId TestId { get; set; }
            public string Title { get; set; }
            public int Percentage { get; set; }
            public ValuationTestQuestionType Type { get; set; }
            public List<ValuationTestQuestionOptionItem> Options { get; set; }
        }

        public class ValuationTestQuestionOptionItem
        {
            public string Text { get; set; }
            public bool Correct { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ValuationTestItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<ValuationTestItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Fail<ValuationTestItem>("Acesso Negado");

                if (string.IsNullOrEmpty(request.TestId))
                    return Result.Fail<ValuationTestItem>("Id do Teste não informado");
                
                var testId = ObjectId.Parse(request.TestId);

                var valuationTest = await _db.ValuationTestCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => 
                        x.Id == testId
                    );
                
                if (valuationTest == null)
                    return Result.Fail<ValuationTestItem>("Teste não existe");

                var questionsCollection = _db.Database.GetCollection<ValuationTestQuestionItem>("ValuationTestQuestions");
                var questionsQuery = await questionsCollection.FindAsync(
                    x => x.TestId == testId,
                    cancellationToken: cancellationToken
                );
                var testQuestions = await questionsQuery.ToListAsync(cancellationToken);

                var test = new ValuationTestItem
                {
                    Id = valuationTest.Id,
                    Title = valuationTest.Title,
                    TestModules = valuationTest.TestModules,
                    TestQuestions = testQuestions,
                    TestTracks = valuationTest.TestTracks
                };

                //if (request.UserRole == "Student")
                //{
                //    foreach (var question in test.TestQuestions)
                //    {
                //        if (question.Options != null)
                //        {
                //            question.Options = question.Options
                //                .Select(q => new ValuationTestQuestionOptionItem {
                //                    Text = q.Text
                //                }).ToList();
                //        }
                //    }
                //}

                return Result.Ok(test);
            }
        }
    }
}
