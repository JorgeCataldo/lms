using System;
using System.Collections.Generic;
using System.Linq;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Questions
{
    public class Question: Entity
    {
        private Question(string text, int level, int duration, ObjectId moduleId, ObjectId subjectId)
        {
            Id = ObjectId.GenerateNewId();
            Text = text;
            Level = level;
            Duration = duration;
            SubjectId = subjectId;
            ModuleId = moduleId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Answers = new List<Answer>();
            Concepts = new List<Concept>();
        }

        private Question(ObjectId questionId, string text, int level, int duration, ObjectId moduleId, ObjectId subjectId)
        {
            Id = questionId;
            Text = text;
            Level = level;
            Duration = duration;
            SubjectId = subjectId;
            ModuleId = moduleId;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Answers = new List<Answer>();
            Concepts = new List<Concept>();
        }

        public string Text { get; set; }
        public List<Answer> Answers { get; set; }
        public List<Concept> Concepts { get; set; }
        public ObjectId SubjectId { get; set; }
        public ObjectId ModuleId { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
        
        public static Result<Question> Create(string text, int level, int duration, string[] concepts, List<Answer> answers, ObjectId moduleId, ObjectId subjectId)
        {
            var newObject = new Question(text, level, duration, moduleId, subjectId);

            var resultList = concepts.Select(Concept.Create).ToArray();
            var results = Result.Combine(resultList);

            if (results.IsFailure)
                return Result.Fail<Question>(results.Error);

            newObject.Concepts = resultList.Select(x => x.Data).ToList();

            var outOfOrderConcepts = answers.SelectMany(x => x.Concepts).Where(x => !concepts.Contains(x.Concept));
            if (outOfOrderConcepts.Any())
                return Result.Fail<Question>(
                    "Existem conceitos presentes nas respostas que não estão contidos na pergunta");
            
            newObject.Answers = answers;

            return Result.Ok(newObject);
        }

        public static Result<Question> Create(string text, int level, int duration, List<Concept> concepts, List<Answer> answers, ObjectId moduleId, ObjectId subjectId)
        {
            var newObject = new Question(text, level, duration, moduleId, subjectId);
            newObject.Concepts = concepts;
            newObject.Answers = answers;
            return Result.Ok(newObject);
        }

        public static Result<Question> Create(ObjectId questionId, string text, int level, int duration, List<Concept> concepts, List<Answer> answers, ObjectId moduleId, ObjectId subjectId)
        {
            var newObject = new Question(questionId, text, level, duration, moduleId, subjectId);
            newObject.Concepts = concepts;
            newObject.Answers = answers;
            return Result.Ok(newObject);
        }
    }
}