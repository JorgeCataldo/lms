using System.Collections.Generic;
using Domain.ValueObjects.Serializers;
using MongoDB.Bson.Serialization.Attributes;
using Tg4.Infrastructure.Functional;
using ValueObject = Domain.SeedWork.ValueObject;

namespace Domain.ValueObjects
{
    [BsonSerializer(typeof(TagSerializer))]
    public class Tag: ValueObject
    {
        public string Name { get; set; }
        
        private Tag(string name)
        {
            Name = name;
        }

        public static Result<Tag> Create(string name)
        {
            return name.Length > 40 ? 
                Result.Fail<Tag>($"Máximo de nome de conceito é de 40 caracteres. ({name})") : 
                Result.Ok(new Tag(name));
        }

        public static Tag Load(string tag)
        {
            return new Tag(tag);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
        }
    }
}