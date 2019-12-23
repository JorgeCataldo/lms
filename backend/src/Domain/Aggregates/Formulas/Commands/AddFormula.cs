using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Formula;
using static Domain.Aggregates.FormulaVariables;

namespace Domain.Aggregates.Formulas.Commands
{
    public class AddFormula
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string Title { get; set; }
            public int Type { get; set; }
            public List<FormulaPartItem> FormulaParts { get; set; }
        }

        public class FormulaPartItem
        {
            public int Order { get; set; }
            public int? Operator { get; set; }
            public string Key { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<Result>("Acesso Negado");
                
                if (request.FormulaParts == null || request.FormulaParts.Count == 0)
                    return Result.Fail<Result>("Fórmula Inválida");

                var countParenthesis = request.FormulaParts.Count(
                    p => p.Operator.HasValue && (
                        p.Operator.Value == (int)FormulaOperator.OpenParenthesis ||
                        p.Operator.Value == (int)FormulaOperator.CloseParenthesis
                    )
                );

                if (countParenthesis % 2 != 0)
                    return Result.Fail<Result>("Fórmula Inválida");

                var formula = Formula.Create(
                    request.Title, (FormulaType)request.Type
                ).Data;
                
                foreach (var part in request.FormulaParts)
                {
                    FormulaOperator? formOperator = null;

                    if (part.Operator.HasValue)
                        formOperator = (FormulaOperator) part.Operator.Value;

                    formula.FormulaParts.Add(
                        new FormulaPart() {
                            Order = part.Order,
                            Key = part.Key,
                            Operator = formOperator
                        }
                    );
                }

                await _db.FormulaCollection.InsertOneAsync(
                    formula, cancellationToken: cancellationToken
                );

                return Result.Ok( formula );
            }
        }
    }
}
