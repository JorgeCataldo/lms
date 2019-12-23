using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Sectors
{
    public class Sector : Entity
    {
        public Sector(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Sector(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Sector> Create(string name)
        {
            var newObject = new Sector(name);
            return Result.Ok(newObject);
        }
     }
}