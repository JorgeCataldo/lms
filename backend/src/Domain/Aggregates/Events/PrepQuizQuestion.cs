using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.SeedWork;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class PrepQuizQuestion: Entity
    {
        private PrepQuizQuestion(string question, bool fileAsAnswer)
        {
            //###ver se é melhor remover o construtor
            Id = ObjectId.GenerateNewId();
            Question = question;
            FileAsAnswer = fileAsAnswer;
        }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId QuestionId { get; set; }
        public string Question { get; set; }
        public bool FileAsAnswer { get; set; }

        public static Result<PrepQuizQuestion> Create(string question, bool fileAsAnswer, string[] questions)
        {
            if(string.IsNullOrWhiteSpace(question))
                return Result.Fail<PrepQuizQuestion>("Não podem haver perguntas em branco");

            if(questions
                .Where(x => x == question).ToList().Count > 1)
                return Result.Fail<PrepQuizQuestion>("Não podem haver perguntas repetidas");
            
            return Result.Ok(new PrepQuizQuestion(question, fileAsAnswer));
        }

    }
}