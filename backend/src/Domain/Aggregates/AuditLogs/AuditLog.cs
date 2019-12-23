using System;
using Domain.SeedWork;
using MongoDB.Bson;
using Newtonsoft.Json;
namespace Performance.Domain.Aggregates.AuditLogs
{
    public enum EntityAction
    {
        Add = 1,
        Update = 2,
        Delete = 3
    }
    public class AuditLog
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId? ImpersonatedBy { get; set; }
        public DateTimeOffset Date { get; set; }
        public EntityAction Action { get; set; }
        public string EntityType { get; set; }
        public ObjectId EntityId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string ActionDescription { get; set; }

        public static AuditLog Create(ObjectId userId, ObjectId entityId, string type, string newValue, EntityAction action, string oldValues = "")
        {
            var log = new AuditLog()
            {
                Action = action,
                Date = DateTimeOffset.UtcNow,
                EntityId = entityId,
                EntityType = type,
                UserId = userId,
                ImpersonatedBy = null
            };
            if (action == EntityAction.Add || action == EntityAction.Update)
            {
                log.NewValues = newValue;
            }
            if (action == EntityAction.Delete || action == EntityAction.Update)
            {
                log.OldValues = oldValues;
            }

            return log;
        }
    }
}
