using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestQuestion;

namespace Domain.Aggregates.ProfileTests.Queries
{
    public class GetProfileTestByIdQuery
    {
        public class Contract : CommandContract<Result<ProfileTestItem>>
        {
            public string TestId { get; set; }
            public string UserRole { get; set; }
        }

        public class ProfileTestItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public bool IsDefault { get; set; }
            public List<ProfileTestQuestionItem> TestQuestions { get; set; }
        }

        public class ProfileTestQuestionItem
        {
            public ObjectId Id { get; set; }
            public ObjectId TestId { get; set; }
            public string Title { get; set; }
            public int Percentage { get; set; }
            public ProfileTestQuestionType Type { get; set; }
            public List<ProfileTestQuestionOptionItem> Options { get; set; }
        }

        public class ProfileTestQuestionOptionItem
        {
            public string Text { get; set; }
            public bool Correct { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ProfileTestItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<ProfileTestItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail<ProfileTestItem>("Acesso Negado");

                if (string.IsNullOrEmpty(request.TestId))
                    return Result.Fail<ProfileTestItem>("Id do Teste não informado");

                var testId = ObjectId.Parse(request.TestId);

                var collection = _db.Database.GetCollection<ProfileTestItem>("ProfileTests");
                var query = await collection.FindAsync(
                    x => x.Id == testId,
                    cancellationToken: cancellationToken
                );

                var test = await query.FirstOrDefaultAsync(cancellationToken);
                if (test == null)
                    return Result.Fail<ProfileTestItem>("Teste não existe");

                var questionsCollection = _db.Database.GetCollection<ProfileTestQuestionItem>("ProfileTestQuestions");
                var questionsQuery = await questionsCollection.FindAsync(
                    x => x.TestId == testId,
                    cancellationToken: cancellationToken
                );

                test.TestQuestions = await questionsQuery.ToListAsync(cancellationToken);

                if (request.UserRole == "Student")
                {
                    foreach (var question in test.TestQuestions)
                    {
                        if (question.Options != null)
                        {
                            question.Options = question.Options
                                .Select(q => new ProfileTestQuestionOptionItem {
                                    Text = q.Text
                                }).ToList();
                        }
                    }
                }

                return Result.Ok(test);
            }
        }
    }
}
