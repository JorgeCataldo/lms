using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Questions
{
    public class QuestionDraft : Entity
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId QuestionId { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId DraftId { get; set; }
        public bool DraftPublished { get; set; }
        public DateTimeOffset DraftPlubishedAt { get; set; }

        public string Text { get; set; }
        public List<Answer> Answers { get; set; }
        public List<Concept> Concepts { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId SubjectId { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId ModuleId { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }

        public static Result<QuestionDraft> Create(
            ObjectId draftId, string text, int level, int duration,
            string[] concepts, List<Answer> answers,
            ObjectId moduleId, ObjectId subjectId
        ) {
            var newObject = new QuestionDraft() {
                Id = ObjectId.GenerateNewId(),
                Text = text,
                Level = level,
                Duration = duration,
                SubjectId = subjectId,
                ModuleId = moduleId,
                CreatedAt = DateTimeOffset.UtcNow,
                Answers = new List<Answer>(),
                Concepts = new List<Concept>(),
                DraftId = draftId,
                DraftPublished = false,
                DraftPlubishedAt = DateTimeOffset.MinValue
            };

            var resultList = concepts.Select(Concept.Create).ToArray();
            var results = Result.Combine(resultList);

            if (results.IsFailure)
                return Result.Fail<QuestionDraft>(results.Error);

            newObject.Concepts = resultList.Select(x => x.Data).ToList();

            var outOfOrderConcepts = answers.SelectMany(x => x.Concepts).Where(x => !concepts.Contains(x.Concept));
            if (outOfOrderConcepts.Any())
                return Result.Fail<QuestionDraft>(
                    "Existem conceitos presentes nas respostas que não estão contidos na pergunta");

            newObject.Answers = answers;

            return Result.Ok(newObject);
        }

        public static Result<QuestionDraft> Create(
            ObjectId questionId, ObjectId draftId, string text, int level, int duration,
            string[] concepts, List<Answer> answers,
            ObjectId moduleId, ObjectId subjectId
        ) {
            var newObject = new QuestionDraft()
            {
                Id = ObjectId.GenerateNewId(),
                QuestionId = questionId,
                Text = text,
                Level = level,
                Duration = duration,
                SubjectId = subjectId,
                ModuleId = moduleId,
                CreatedAt = DateTimeOffset.UtcNow,
                Answers = new List<Answer>(),
                Concepts = new List<Concept>(),
                DraftId = draftId,
                DraftPublished = false,
                DraftPlubishedAt = DateTimeOffset.MinValue
            };

            var resultList = concepts.Select(Concept.Create).ToArray();
            var results = Result.Combine(resultList);

            if (results.IsFailure)
                return Result.Fail<QuestionDraft>(results.Error);

            newObject.Concepts = resultList.Select(x => x.Data).ToList();

            var outOfOrderConcepts = answers.SelectMany(x => x.Concepts).Where(x => !concepts.Contains(x.Concept));
            if (outOfOrderConcepts.Any())
                return Result.Fail<QuestionDraft>(
                    "Existem conceitos presentes nas respostas que não estão contidos na pergunta");

            newObject.Answers = answers;

            return Result.Ok(newObject);
        }

        public static Result<QuestionDraft> Create(
            ObjectId questionId, ObjectId draftId, string text, int level, int duration,
            List<Concept> concepts, List<Answer> answers,
            ObjectId moduleId, ObjectId subjectId
        ) {
            var newObject = new QuestionDraft() {
                Id = ObjectId.GenerateNewId(),
                QuestionId = questionId,
                Text = text,
                Level = level,
                Duration = duration,
                SubjectId = subjectId,
                ModuleId = moduleId,
                CreatedAt = DateTimeOffset.UtcNow,
                Answers = new List<Answer>(),
                Concepts = new List<Concept>(),
                DraftId = draftId,
                DraftPublished = false,
                DraftPlubishedAt = DateTimeOffset.MinValue
            };
            newObject.Concepts = concepts;
            newObject.Answers = answers;
            return Result.Ok(newObject);
        }
    }
}