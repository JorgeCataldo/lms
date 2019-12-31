using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.RecruitmentFavorite
{
    public class RecruitmentFavorite : Entity, IAggregateRoot
    {
        public ObjectId RecruiterId { get; set; }
        public ObjectId UserId { get; set; }
        public string UserName { get; set; }
        public string UserImgUrl { get; set; }

        public static Result<RecruitmentFavorite> Create(
            ObjectId recruiterId, ObjectId userId,
            string name, string imgUrl
        ) {
            return Result.Ok(
                new RecruitmentFavorite() {
                    Id = ObjectId.GenerateNewId(),
                    RecruiterId = recruiterId,
                    UserId = userId,
                    UserName = name,
                    UserImgUrl = imgUrl
                }
            );
        }
    }
}