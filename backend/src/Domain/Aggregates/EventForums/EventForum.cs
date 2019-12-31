using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums
{
    public class EventForum : Entity
    {
        public ObjectId EventId { get; set; }
        public ObjectId EventScheduleId { get; set; }
        public string EventName { get; set; }
        public List<ObjectId> Questions { get; set; }

        public static Result<EventForum> Create(
            ObjectId eventId, ObjectId eventScheduleId, 
            string eventName, List<ObjectId> questions
        ) {

            var module = new EventForum()
            {
                Id = ObjectId.GenerateNewId(),
                EventId = eventId,
                EventScheduleId = eventScheduleId,
                EventName = eventName,
                Questions = questions
            };

            return Result.Ok(module);
        }

        private EventForum() : base()
        {
            Questions = new List<ObjectId>();
        }
    }
}