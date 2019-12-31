using MongoDB.Bson;
using System;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Candidate
{
    public class Candidate
    {
        public ObjectId JobPositionId { get; protected set; }
        public ObjectId UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ObjectId CreatedBy { get; set; }
        public bool? Approved { get; set; }
        public bool Accepted { get; set; }

        public static Result<Candidate> Create(
            ObjectId jobPositionId, ObjectId userId, string userName, ObjectId createdBy
        ) {
            return Result.Ok(
                new Candidate() {
                    JobPositionId = jobPositionId,
                    UserId = userId,
                    UserName = userName,
                    CreatedAt = DateTimeOffset.Now,
                    CreatedBy = createdBy,
                    Approved = null,
                    Accepted = false
                }
            );
        }
    }
}