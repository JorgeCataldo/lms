using Domain.SeedWork;
using MongoDB.Bson;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.FormulaVariables;

namespace Domain.Aggregates
{
    public class Formula : Entity, IAggregateRoot
    {
        public string Title { get; set; }
        public FormulaType Type { get; set; }
        public List<FormulaPart> FormulaParts { get; set; }

        public static Result<Formula> Create(
            string title, FormulaType type, List<FormulaPart> parts = null
        ) {
            return Result.Ok(
                new Formula() {
                    Id = ObjectId.GenerateNewId(),
                    FormulaParts = parts ?? new List<FormulaPart>(),
                    Title = title,
                    Type = type
                }
            );
        }

        public class FormulaPart
        {
            public int Order { get; set; }
            public FormulaOperator? Operator { get; set; }
            public string Key { get; set; }
            public int? IntegralNumber { get; set; }
        }

        public enum FormulaOperator
        {
            Plus = 1,
            Minus = 2,
            Times = 3,
            Divided = 4,
            OpenParenthesis = 5,
            CloseParenthesis = 6
        }
    }
}