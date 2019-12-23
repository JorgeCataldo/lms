using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules
{
    public class ModuleGrade : Entity, IAggregateRoot
    {
        public ObjectId UserId { get; set; }
        public ObjectId ModuleId { get; set; }
        public decimal Grade { get; set; }
        public decimal? UserPresence { get; set; }
        
        public static Result<ModuleGrade> Create(
            ObjectId moduleId, ObjectId userId,
            decimal grade, decimal? userPresence
        ) {
            return Result.Ok(
                new ModuleGrade()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = userId,
                    ModuleId = moduleId,
                    Grade = grade,
                    UserPresence = userPresence
                }
            );
        }
    }
}
