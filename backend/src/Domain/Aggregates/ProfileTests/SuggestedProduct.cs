using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests
{
    public class SuggestedProduct : Entity, IAggregateRoot
    {
        public ObjectId UserId { get; set; }
        public ObjectId ProductId { get; set; }
        public SuggestedProductType Type { get; set; }

        public static Result<SuggestedProduct> Create(
            ObjectId productId, SuggestedProductType type,
            ObjectId userId
        ) {
            return Result.Ok(
                new SuggestedProduct() {
                    Id = ObjectId.GenerateNewId(),
                    ProductId = productId,
                    Type = type,
                    UserId = userId
                }
            );
        }

        public enum SuggestedProductType
        {
            Module = 1,
            Event = 2,
            Track = 3
        }
    }
}