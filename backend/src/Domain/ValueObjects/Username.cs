using Tg4.Infrastructure.Functional;

namespace Domain.ValueObjects
{
    public class Username
    {
        private Username(string username)
        {
            Value = username;
        }

        public string Value { get; set; }

        public static Result<Username> Create(string username)
        {
            return Result.Ok(new Username(username));
        }
    }
}