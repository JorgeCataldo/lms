using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums
{
    public class ForumQuestion : Entity
    {
        public ObjectId ModuleId { get; set; }
        public ObjectId? SubjectId { get; set; }
        public string SubjectName { get; set; }
        public ObjectId? ContentId { get; set; }
        public string ContentName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> LikedBy { get; set; }
        public List<ForumAnswer> Answers { get; set; }
        public string Position { get; set; }

        public static Result<ForumQuestion> Create(
            ObjectId moduleId, ObjectId userId,
            string title, string description, List<string> likedBy,
            ObjectId? subjectId = null, string subjectName = null,
            ObjectId? contentId = null, string contentName = null,
            string position = ""
        ) {
            return Result.Ok(
                new ForumQuestion() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    ModuleId = moduleId,
                    SubjectId = subjectId,
                    SubjectName = subjectName,
                    ContentId = contentId,
                    ContentName = contentName,
                    Title = title,
                    Description = description,
                    LikedBy = likedBy,
                    Position = position
                }
            );
        }

        private ForumQuestion() : base()
        {
            LikedBy = new List<string>();
        }
    }
}