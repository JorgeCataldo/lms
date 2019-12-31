using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests
{
    public class ProfileTestQuestion : Entity, IAggregateRoot
    {
        public ObjectId TestId { get; set; }
        public string Title { get; set; }
        public ProfileTestQuestionType Type { get; set; }
        public List<ProfileTestQuestionOption> Options { get; set; }
        public int Percentage { get; set; }

        public static Result<ProfileTestQuestion> Create(
            ObjectId TestId, string title, int percentage,
            ProfileTestQuestionType type,
            List<ProfileTestQuestionOption> options
        ) {
            return Result.Ok(
                new ProfileTestQuestion() {
                    Id = ObjectId.GenerateNewId(),
                    Percentage = percentage,
                    TestId = TestId,
                    Title = title,
                    Type = type,
                    Options = options
                }
            );
        }

        private ProfileTestQuestion() : base()
        {
            Options = new List<ProfileTestQuestionOption>();
        }

        public class ProfileTestQuestionOption
        {
            public string Text { get; set; }
            public bool Correct { get; set; }
        }

        public enum ProfileTestQuestionType
        {
            MultipleChoice = 1,
            Discursive = 2
        }
    }
}