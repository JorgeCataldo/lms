using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ValuationTests
{
    public class ValuationTestQuestion: Entity, IAggregateRoot
    {
        public ObjectId TestId { get; set; }
        public string Title { get; set; }
        public ValuationTestQuestionType Type { get; set; }
        public List<ValuationTestQuestionOption> Options { get; set; }
        public int Percentage { get; set; }

        public static Result<ValuationTestQuestion> Create(
            ObjectId TestId, string title, int percentage,
            ValuationTestQuestionType type,
            List<ValuationTestQuestionOption> options
        ) {
            return Result.Ok(
                new ValuationTestQuestion() {
                    Id = ObjectId.GenerateNewId(),
                    Percentage = percentage,
                    TestId = TestId,
                    Title = title,
                    Type = type,
                    Options = options
                }
            );
        }

        private ValuationTestQuestion() : base()
        {
            Options = new List<ValuationTestQuestionOption>();
        }

        public class ValuationTestQuestionOption
        {
            public string Text { get; set; }
            public bool Correct { get; set; }
        }

        public enum ValuationTestQuestionType
        {
            MultipleChoice = 1,
            Discursive = 2
        }
    }
}