using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Locations
{
    public class Location : Entity
    {
        public Location(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Location(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public Location(ObjectId id, string name, ObjectId countryId)
        {
            Id = id;
            Name = name;
            CountryId = countryId;
        }

        public string Name { get; set; }
        public ObjectId CountryId { get; set; }

        public static Result<Location> Create(string name)
        {
            var newObject = new Location(name);
            return Result.Ok(newObject);
        }
     }
}