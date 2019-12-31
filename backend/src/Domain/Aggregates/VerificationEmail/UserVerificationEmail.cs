using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.VerificationEmail
{
    public class UserVerificationEmail : Entity
    {
        public ObjectId UserId { get; set; }
        public string Code { get; set; }

        public static Result<UserVerificationEmail> Create(ObjectId userId) {
            return Result.Ok(
                new UserVerificationEmail() {
                    Id = ObjectId.GenerateNewId(),
                    UserId = userId,
                    Code = new Random().Next(10000, 99999).ToString(),
                    CreatedAt = DateTimeOffset.Now
                }
            );
        }
    }
}