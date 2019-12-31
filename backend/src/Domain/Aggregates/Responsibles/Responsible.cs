using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Responsibles
{
    public class Responsible
    {
        public ObjectId ResponsibleUserId { get; set; }
        public List<ObjectId> SubordinatesUsersIds { get; set; }

        public static Result<Responsible> Create(ObjectId userId, List<ObjectId> userIds) {
            return Result.Ok(
                new Responsible() {
                    ResponsibleUserId = userId,
                    SubordinatesUsersIds = userIds
                }
            );
        }
    }
}