using Domain.SeedWork;
using MongoDB.Bson;
using System;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.PurchaseHistory
{
    public class PurchaseHistory : Entity, IAggregateRoot
    {
        public ProducTypeEnum ProductType { get; set; }
        public ObjectId ProductId { get; set; }
        public DateTimeOffset? PurchaseDate { get; set; }

        public static Result<PurchaseHistory> Create(
            ProducTypeEnum type, ObjectId productId, ObjectId userId,
            DateTimeOffset? purchaseDate = null
        ) {
            return Result.Ok(
                new PurchaseHistory() {
                    Id = ObjectId.GenerateNewId(),
                    ProductType = type,
                    ProductId = productId,
                    CreatedBy = userId,
                    PurchaseDate = purchaseDate ?? DateTimeOffset.MinValue
                }
            );
        }

        public enum ProducTypeEnum
        {
            Track = 1,
            Module = 2,
            Event = 3
        }
    }
}