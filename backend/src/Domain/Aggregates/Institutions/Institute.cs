using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Institutions
{
    public class Institute : Entity
    {
        public string Name { get; set; }
        public InstituteType Type { get; set; }

        public static Result<Institute> Create(string name, InstituteType type) {
            return Result.Ok(
                new Institute()
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = name,
                    Type = type
                }
            );
        }

        public enum InstituteType
        {
            School = 1,
            College = 2
        }
    }
}