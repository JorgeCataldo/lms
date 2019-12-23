using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Messages
{
    public class ContactArea
    {
        public ObjectId Id { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }

        public static Result<ContactArea> Create(
            string description, string email
        ) {
            return Result.Ok(
                new ContactArea()
                {
                    Id = ObjectId.GenerateNewId(),
                    Description = description,
                    Email = email
                }
            );
        }
    }
}