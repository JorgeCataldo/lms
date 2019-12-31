using System;
using Domain.SeedWork;
using MongoDB.Bson;

namespace Domain.Aggregates.UserProgressHistory
{
    public class UserModuleProgress : Entity
    {
        public ObjectId ModuleId { get; set; }
        public ObjectId UserId { get; set; }
        public int Level { get; set; }
        public decimal Progress { get; set; }
        public int Points { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
        public int? ValidFor { get; set; }
        public DateTimeOffset? DueDate { get; set; }

        private UserModuleProgress(ObjectId moduleId, ObjectId userId, int level, decimal progress, int points)
        {
            Id = ObjectId.GenerateNewId();
            ModuleId = moduleId;
            UserId = userId;
            Level = level;
            Progress = progress;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Points = points;
        }

        public static UserModuleProgress Create(ObjectId moduleId, ObjectId userId)
        {
            return new UserModuleProgress(moduleId, userId, 0, 0, 0);
        }
    }
}
