using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.BusinessGroups
{
    public class BusinessGroup : Entity
    {
        public BusinessGroup(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public BusinessGroup(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<BusinessGroup> Create(string name)
        {
            var newObject = new BusinessGroup(name);
            return Result.Ok(newObject);
        }
     }
}