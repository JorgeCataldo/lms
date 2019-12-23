using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Messages
{
    public class CustomEmail : Entity
    {
        public List<ObjectId> UsersIds { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }

        public static Result<CustomEmail> Create(ObjectId userId, List<ObjectId> usersIds,
            string title, string text
        ) {
            return Result.Ok(
                new CustomEmail() {
                    Id = ObjectId.GenerateNewId(),
                    UsersIds = usersIds,
                    Title = title,
                    Text = text,
                    CreatedAt = DateTimeOffset.Now,
                    CreatedBy = userId
                }
            );
        }

        public static List<string> GetPossibleEmailVariables()
        {
            var variables = new List<string>
            {
                "--Nome--"
            };
            return variables;
        }
    }
}