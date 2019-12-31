using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests
{
    public class ProfileTest : Entity, IAggregateRoot
    {
        public string Title { get; set; }
        public List<ObjectId> Questions { get; set; }
        public bool IsDefault { get; set; }

        public static Result<ProfileTest> Create(
            string title, List<ObjectId> questions,
            bool isDefault
        ) {

            var module = new ProfileTest()
            {
                Id = ObjectId.GenerateNewId(),
                Title = title,
                Questions = questions,
                IsDefault = isDefault
            };

            return Result.Ok(module);
        }

        private ProfileTest() : base()
        {
            Questions = new List<ObjectId>();
        }
    }
}