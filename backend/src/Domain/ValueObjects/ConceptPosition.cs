using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using ValueObject = Domain.SeedWork.ValueObject;

namespace Domain.ValueObjects
{
    public class ConceptPosition: ValueObject
    {
        public string Name { get; set; }
        public List<long> Positions { get; set; }
        public List<string> Anchors { get; set; }
        
        public ConceptPosition(string name, List<long> positions, List<string> anchors)
        {
            Name = name;
            Positions = positions;
            Anchors = anchors;
        }

        public static Result<ConceptPosition> Create(string name, List<long> positions, List<string> anchors)
        {
            return name.Length > 200 ? 
                Result.Fail<ConceptPosition>($"Máximo de nome de conceito é de 200 caracteres. ({name})") : 
                Result.Ok(new ConceptPosition(name, positions, anchors));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return Positions;
            yield return Anchors;
        }
    }
}