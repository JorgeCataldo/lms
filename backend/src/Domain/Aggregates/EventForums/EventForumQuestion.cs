using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums
{
    public class EventForumQuestion : Entity
    {
        public ObjectId EventId { get; set; }
        public ObjectId EventScheduleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> LikedBy { get; set; }
        public List<EventForumAnswer> Answers { get; set; }
        public string Position { get; set; }

        public static Result<EventForumQuestion> Create(
            ObjectId eventId, ObjectId eventScheduleId, ObjectId userId,
            string title, string description, List<string> likedBy,
            string position = ""
        ) {
            return Result.Ok(
                new EventForumQuestion() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    EventId = eventId,
                    EventScheduleId = eventScheduleId,
                    Title = title,
                    Description = description,
                    LikedBy = likedBy,
                    Position = position
                }
            );
        }

        private EventForumQuestion() : base()
        {
            LikedBy = new List<string>();
        }
    }
}