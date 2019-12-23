using Domain.ValueObjects.Serializers;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using ValueObject = Domain.SeedWork.ValueObject;

namespace Domain.ValueObjects
{
    [BsonSerializer(typeof(ConceptSerializer))]
    public class Concept: ValueObject
    {
        public string Name { get; set; }
        
        public Concept(string name)
        {
            Name = name;
        }

        public static Concept Load(string concept)
        {
            return new Concept(concept);
        }

        public static Result<Concept> Create(string name)
        {
            return name.Length > 200 ? 
                Result.Fail<Concept>($"Máximo de nome de conceito é de 200 caracteres. ({name})") : 
                Result.Ok(new Concept(name));
        }

        public static Result<List<Concept>> GetConcepts(string[] concepts)
        {
            var clist = new List<Concept>();

            if (concepts != null)
            {
                foreach (var concept in concepts)
                {
                    var res = Concept.Create(concept);
                    if (res.IsFailure)
                    {
                        return Result.Fail<List<Concept>>(res.Error);
                    }

                    clist.Add(res.Data);
                }
            }

            return Result.Ok(clist);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
        }
    }
}