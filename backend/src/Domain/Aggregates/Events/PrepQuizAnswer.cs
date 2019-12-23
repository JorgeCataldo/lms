using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;
using Domain.SeedWork;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class PrepQuizAnswer: Entity
    {
        private PrepQuizAnswer(string answer, bool fileAsAnswer)
        {
            Id = ObjectId.GenerateNewId();
            Answer = answer;
            FileAsAnswer = fileAsAnswer;
        }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId AnswerId { get; set; }
        public string Answer { get; set; }
        public bool FileAsAnswer { get; set; }

        public static Result<PrepQuizAnswer> Create(string answer, bool fileAsAnswer)
        {
            if(string.IsNullOrWhiteSpace(answer))
                return Result.Fail<PrepQuizAnswer>("Não podem haver respostas em branco");
                        
            return Result.Ok(new PrepQuizAnswer(answer, fileAsAnswer));
        }

    }
}