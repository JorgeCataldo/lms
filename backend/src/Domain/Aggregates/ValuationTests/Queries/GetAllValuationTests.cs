using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.Enumerations;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTest;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetAllValuationTests
    {
        public class Contract : CommandContract<Result<List<ValuationTestItem>>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class ValuationTestItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public ValuationTestTypeEnum Type { get; set; }
            public List<ObjectId> Questions { get; set; }
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

        public class Handler : IRequestHandler<Contract, Result<List<ValuationTestItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ValuationTestItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Fail<List<ValuationTestItem>>("Acesso Negado");

                var valuationTestItems = new List<ValuationTestItem>();

                var userId = ObjectId.Parse(request.UserId);


                var testIds = new List<ObjectId>();
                var tests = await _db.ValuationTestCollection
                    .AsQueryable()
                    .ToListAsync();

                if (tests.Count == 0)
                    return Result.Ok(valuationTestItems);

                testIds = tests.Select(x => x.Id).ToList();

                var questionsCollection = _db.Database.GetCollection<ValuationTestQuestionItem>("ValuationTestQuestions");
                var questionsQuery = await questionsCollection.FindAsync(
                    x => testIds.Contains(x.TestId),
                    cancellationToken: cancellationToken
                );
                var testQuestions = await questionsQuery.ToListAsync(cancellationToken);

                foreach (var test in tests)
                {
                    var valTest = new ValuationTestItem
                    {
                        Id = test.Id,
                        Title = test.Title,
                        Type = test.Type,
                        Questions = testQuestions.Where(x => x.TestId == test.Id).Select(x => x.Id).ToList(),
                        TestQuestions = testQuestions.Where(x => x.TestId == test.Id).ToList(),
                        TestModules = test.TestModules,
                        TestTracks = test.TestTracks,
                    };
                    valuationTestItems.Add(valTest);
                }

                return Result.Ok(valuationTestItems);
            }
        }
    }
}
