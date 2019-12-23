using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Messages
{
    public class Message : Entity
    {
        public ObjectId ContactAreaId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string FileUrl { get; set; }

        public static Result<Message> Create(
            ObjectId userId, ObjectId contactAreaId,
            string title, string text, string fileUrl
        ) {
            return Result.Ok(
                new Message() {
                    Id = ObjectId.GenerateNewId(),
                    ContactAreaId = contactAreaId,
                    Title = title,
                    Text = text,
                    FileUrl = fileUrl,
                    CreatedBy = userId
                }
            );
        }
    }
}