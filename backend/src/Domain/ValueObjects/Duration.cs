using Domain.ValueObjects.Serializers;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.ValueObjects
{
    [BsonSerializer(typeof(DurationSerializer))]
    public class Duration: ValueObject
    {
        public int Minutes { get; private set; }

        public Duration(int minutes)
        {
            this.Minutes = minutes;
        }

        public static Duration Load(int duration)
        {
            return new Duration(duration);
        }

        public static Result<Duration> Create(int minutes)
        {
            if (minutes < 0)
                return Result.Fail<Duration>("Duração deve ser maior que zero");

            return Result.Ok(new Duration(minutes));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Minutes;
        }
    }
}