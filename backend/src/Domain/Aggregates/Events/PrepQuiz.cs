using System.Collections.Generic;
using System.Linq;
using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class PrepQuiz: Entity
    {
        private PrepQuiz(List<string> questions)
        {
            Id = ObjectId.GenerateNewId();
            Questions = questions;
        }

        public List<string> Questions { get; set; }

        public static Result<PrepQuiz> Create(string[] questions)
        {
            if(questions.Any(string.IsNullOrWhiteSpace))
                return Result.Fail<PrepQuiz>("Não podem haver perguntas em branco");

            if(questions
                .GroupBy(x=>x)
                .Select(x=>new {x.Key,count= x.Count()})
                .Any(x=>x.count > 1))
                return Result.Fail<PrepQuiz>("Não podem haver perguntas repetidas");

            return Result.Ok(new PrepQuiz(questions.ToList()));
        }

    }
}