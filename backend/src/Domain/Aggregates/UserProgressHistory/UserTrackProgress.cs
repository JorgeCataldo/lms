using System;
using System.Collections.Generic;
using System.Text;
using Domain.SeedWork;
using MongoDB.Bson;

namespace Domain.Aggregates.UserProgressHistory
{
    public class UserTrackProgress : Entity
    {
        public ObjectId TrackId { get; set; }
        public ObjectId UserId { get; set; }
        public int Level { get; set; }
        public decimal Progress { get; set; }
        public List<ObjectId> ModulesCompleted { get; set; }
        public DateTimeOffset CompletedAt { get; set; }

        public UserTrackProgress(ObjectId trackId, ObjectId userId, int level, decimal progress)
        {
            TrackId = trackId;
            UserId = userId;
            Level = level;
            Progress = progress;
            CreatedAt = DateTimeOffset.UtcNow;
            ModulesCompleted = new List<ObjectId>();
        }
    }
}
