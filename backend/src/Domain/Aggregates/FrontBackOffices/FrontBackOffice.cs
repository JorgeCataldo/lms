using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.FrontBackOffices
{
    public class FrontBackOffice : Entity
    {
        public FrontBackOffice(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public FrontBackOffice(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<FrontBackOffice> Create(string name)
        {
            var newObject = new FrontBackOffice(name);
            return Result.Ok(newObject);
        }
     }
}