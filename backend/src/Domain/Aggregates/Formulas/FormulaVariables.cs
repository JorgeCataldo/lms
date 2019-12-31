using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates
{
    public class FormulaVariables : Entity, IAggregateRoot
    {
        public FormulaType Type { get; set; }
        public List<string> Variables { get; set; }

        public static Result<FormulaVariables> Create(
            FormulaType type, List<string> variables = null
        ) {
            return Result.Ok(
                new FormulaVariables() {
                    Id = ObjectId.GenerateNewId(),
                    Type = type,
                    Variables = variables ?? new List<string>()
                }
            );
        }

        public enum FormulaType
        {
            EventsParticipation = 1,
            EventsFinalGrade = 2,
            ModuleGrade = 3
        }
    }
}