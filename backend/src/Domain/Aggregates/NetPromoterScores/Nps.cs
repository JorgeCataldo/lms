using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.NetPromoterScores
{
    public class NetPromoterScore : Entity, IAggregateRoot
    {
        public ObjectId UserId { get; set; }
        public decimal Grade { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<UserProgress> TracksInfo { get; set; }
        public List<UserProgress> ModulesInfo { get; set; }
        public List<UserProgress> EventsInfo { get; set; }

        public static Result<NetPromoterScore> Create(
            ObjectId userId, decimal grade, string name, string email,
            List<UserProgress> tracksInfo, List<UserProgress> modulesInfo, List<UserProgress> eventsInfo
        ) {
            return Result.Ok(
                new NetPromoterScore()
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = userId,
                    Grade = grade,
                    Name = name,
                    Email =  email,
                    TracksInfo = tracksInfo,
                    ModulesInfo = modulesInfo,
                    EventsInfo = eventsInfo
                }
            );
        }
    }
}
