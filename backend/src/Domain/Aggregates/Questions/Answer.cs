using System.Collections.Generic;
using Domain.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Domain.Aggregates.Questions
{
    public class Answer
    {
        public Answer(string description, int points, List<AnswerConcept> concepts)
        {
            Id = ObjectId.GenerateNewId();
            Description = description;
            Points = points;
            Concepts = concepts ?? new List<AnswerConcept>();
        }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public List<AnswerConcept> Concepts { get; set; }
    }
}