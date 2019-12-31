using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Ranks
{
    public class Rank : Entity
    {
        public Rank(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Rank(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Rank> Create(string name)
        {
            var newObject = new Rank(name);
            return Result.Ok(newObject);
        }
     }
}