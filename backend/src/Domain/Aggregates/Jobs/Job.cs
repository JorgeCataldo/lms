using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Jobs
{
    public class Job : Entity
    {
        public Job(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Job(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Job> Create(string name)
        {
            var newObject = new Job(name);
            return Result.Ok(newObject);
        }
     }
}