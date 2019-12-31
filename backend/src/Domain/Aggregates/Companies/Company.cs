using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Companies
{
    public class Company : Entity
    {
        public Company(string name)
        {
            Id = ObjectId.GenerateNewId();
            Name = name;
        }

        public Company(ObjectId id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public static Result<Company> Create(string name)
        {
            var newObject = new Company(name);
            return Result.Ok(newObject);
        }
     }
}