using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates
{
    public class ErrorLog : Entity
    {
        public ErrorLogTypeEnum Type { get; set; }
        public string Tag { get; set; }
        public string ErrorJson { get; set; }

        public static Result<ErrorLog> Create(
            ErrorLogTypeEnum type, string tag, string errorJson
        ) {
            return Result.Ok(
                new ErrorLog() {
                    Id = ObjectId.GenerateNewId(),
                    Tag = tag,
                    Type = type,
                    ErrorJson = errorJson
                } 
            );
        }
    }

    public enum ErrorLogTypeEnum
    {
        EcommerceIntegration = 1,
        DatabaseImport = 2,
        PagarMeIntegration = 3
    }
}