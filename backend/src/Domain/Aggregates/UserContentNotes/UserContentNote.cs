using System;
using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserContentNotes
{
    public class UserContentNote : Entity
    {
        public ObjectId UserId { get; set; }
        public ObjectId ModuleId { get; set; }
        public ObjectId SubjectId { get; set; }
        public ObjectId ContentId { get; set; }
        public string Note { get; set; }

        public UserContentNote() { }

        private UserContentNote(ObjectId userId, ObjectId moduleId, ObjectId subjectId, 
            ObjectId contentId, string note)
        {
            Id = ObjectId.GenerateNewId();
            UserId = userId;
            ModuleId = moduleId;
            SubjectId = subjectId;
            ContentId = contentId;
            Note = note;
        }

        public static Result<UserContentNote> Create(ObjectId userId, ObjectId moduleId, 
            ObjectId subjectId, ObjectId contentId, string note = "")
        {
            if (!string.IsNullOrEmpty(note) && note.Length > 4000)
            {
                return Result.Fail<UserContentNote>("A nota não pode possuir mais de 4000 caracteres");
            }
            return Result.Ok(new UserContentNote(userId, moduleId, subjectId, contentId, note));
        }
    }
}
