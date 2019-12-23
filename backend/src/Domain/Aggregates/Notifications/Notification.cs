using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Notifications
{
    public class Notification : Entity, IAggregateRoot
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string RedirectPath { get; set; }
        public ObjectId UserId { get; set; }
        public bool Read { get; set; }
        public bool EmailDelivered { get; set; }

        public static Result<Notification> Create(
            ObjectId userId, bool emailDelivered,
            string title, string text, string redirectPath = ""
        ) {
            return Result.Ok(
                new Notification()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = userId,
                    Title = title,
                    Text = text,
                    RedirectPath = redirectPath,
                    Read = false,
                    EmailDelivered = emailDelivered
                }
            );
        }
    }
}
