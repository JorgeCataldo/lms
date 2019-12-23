using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Countries
{
    public class Country : Entity
    {
        public Country(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Country(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Country> Create(string name)
        {
            var newObject = new Country(name);
            return Result.Ok(newObject);
        }
     }
}