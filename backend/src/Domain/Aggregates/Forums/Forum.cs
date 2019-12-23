using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums
{
    public class Forum : Entity
    {
        public ObjectId ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<ObjectId> Questions { get; set; }

        public static Result<Forum> Create(
            ObjectId moduleId, string moduleName,
            List<ObjectId> questions
        ) {

            var module = new Forum()
            {
                Id = ObjectId.GenerateNewId(),
                ModuleId = moduleId,
                ModuleName = moduleName,
                Questions = questions
            };

            return Result.Ok(module);
        }

        private Forum() : base()
        {
            Questions = new List<ObjectId>();
        }
    }
}