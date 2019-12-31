using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;

namespace Domain.Aggregates.ValuationTests
{
    public class ValuationTestResponse : Entity, IAggregateRoot
    {
        public string UserName { get; set; }
        public string UserRegisterId { get; set; }
        public ObjectId TestId { get; set; }
        public string TestTitle { get; set; }
        public List<ValuationTestAnswer> Answers { get; set; }

        public static Result<ValuationTestResponse> Create(
            ObjectId testId, string title,
            ObjectId userId, string userName, string userRegisterId,
            List<ValuationTestAnswer> answers
        ) {
            return Result.Ok(
                new ValuationTestResponse()
                {
                    Id = ObjectId.GenerateNewId(),
                    TestId = testId,
                    TestTitle = title,
                    CreatedBy = userId,
                    UserName = userName,
                    UserRegisterId = userRegisterId,
                    Answers = answers
                }
            );
        }

        public class ValuationTestAnswer
        {
            public ObjectId QuestionId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public decimal? Grade { get; set; }
            public int? Percentage { get; set; }
            public ValuationTestQuestionType? Type { get; set; }
        }
    }
}