using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Segments
{
    public class Segment : Entity
    {
        public Segment(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Segment(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Segment> Create(string name)
        {
            var newObject = new Segment(name);
            return Result.Ok(newObject);
        }
     }
}