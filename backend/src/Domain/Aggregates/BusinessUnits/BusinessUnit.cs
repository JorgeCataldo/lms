using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.BusinessUnits
{
    public class BusinessUnit : Entity
    {
        public BusinessUnit(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public BusinessUnit(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<BusinessUnit> Create(string name)
        {
            var newObject = new BusinessUnit(name);
            return Result.Ok(newObject);
        }
     }
}