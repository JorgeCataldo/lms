using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums
{
    public class ForumAnswer : Entity
    {   
        public ObjectId QuestionId { get; set; }
        public string Text { get; set; }
        public List<string> LikedBy { get; set; }
        public string UserName { get; set; }
        public string UserImgUrl { get; set; }

        public static Result<ForumAnswer> Create(
            ObjectId questionId, ObjectId userId,
            string userName, string userImgUrl,
            string text, List<string> likedBy
        ) {
            return Result.Ok(
                new ForumAnswer()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    QuestionId = questionId,
                    UserName = userName,
                    UserImgUrl = userImgUrl,
                    LikedBy = likedBy,
                    Text = text
                }
            );
        }

        private ForumAnswer() : base()
        {
            LikedBy = new List<string>();
        }
    }
}