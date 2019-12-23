using System.Collections.Generic;
using Domain.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;
using ValueObject = Domain.SeedWork.ValueObject;

namespace Domain.ValueObjects
{
    public class Requirement : ValueObject
    {

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId ModuleId { get; set; }
        public UserProgress RequirementValue { get; set; }
        public bool Optional { get; set; }

        private Requirement(ObjectId moduleId, bool optional, UserProgress requirementValue)
        {
            ModuleId = moduleId;
            Optional = optional;
            RequirementValue = requirementValue;
        }

        public static Result<Requirement> Create(ObjectId moduleId, bool optional, int level, decimal percentage, ProgressType progressType)
        {
            var result = UserProgress.Create(progressType, level, percentage);
            if (result.IsFailure)
                return Result.Fail<Requirement>(result.Error);

            return Result.Ok(new Requirement(moduleId, optional, result.Data));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            throw new System.NotImplementedException();
        }
    }

}