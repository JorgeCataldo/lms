using Domain.Enumerations;
using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules
{
    public class Action : Entity
    {
        public ActionPage Page { get; set; }
        public string Description { get; set; }
        public ActionType Type { get; set; }
        public string ModuleId { get; set; }
        public string EventId { get; set; }
        public string SubjectId { get; set; }
        public string ContentId { get; set; }
        public string Concept { get; set; }
        public string SupportMaterialId { get; set; }
        public string QuestionId { get; set; }

        public static Result<Action> Create(
            ActionPage page, string description, ActionType type, string userId,
            string moduleId = null, string eventId = null,
            string subjectId = null, string contentId = null,
            string concept = null, string supportMaterialId = null, string questionId = null
        ) {
            if (description.Length > 200)
                return Result.Fail<Action>($"Tamanho máximo da descrição da action é de 200 caracteres.");

            var module = new Action()
            {
                Id = ObjectId.GenerateNewId(),
                Description = description,
                Page = page,
                Type = type,
                ModuleId = moduleId,
                EventId = eventId,
                SubjectId = subjectId,
                ContentId = contentId,
                Concept = concept,
                SupportMaterialId = supportMaterialId,
                QuestionId = questionId,
                CreatedBy = ObjectId.Parse(userId)
            };

            return Result.Ok(module);
        }
        
        private Action() : base() { }
    }
}