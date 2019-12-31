using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests
{
    public class ProfileTestResponse : Entity, IAggregateRoot
    {
        public string UserName { get; set; }
        public string UserRegisterId { get; set; }
        public ObjectId TestId { get; set; }
        public string TestTitle { get; set; }
        public List<ProfileTestAnswer> Answers { get; set; }
        public bool Recommended { get; set; }

        public static Result<ProfileTestResponse> Create(
            ObjectId testId, string title,
            ObjectId userId, string userName, string userRegisterId,
            List<ProfileTestAnswer> answers
        ) {
            return Result.Ok(
                new ProfileTestResponse()
                {
                    Id = ObjectId.GenerateNewId(),
                    TestId = testId,
                    TestTitle = title,
                    CreatedBy = userId,
                    UserName = userName,
                    UserRegisterId = userRegisterId,
                    Answers = answers,
                    Recommended = false
                }
            );
        }

        public class ProfileTestAnswer
        {
            public ObjectId QuestionId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public decimal? Grade { get; set; }
        }
    }
}