using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.FormulaVariables;

namespace Domain.Aggregates.Formulas.Commands
{
    public class ManageFormulaTypeVariables
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public int FormulaType { get; set; }
            public List<string> Variables { get; set; }
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

                if (request.FormulaType < 1)
                    return Result.Fail<Result>("Acesso Negado");

                var formulaType = (FormulaType)request.FormulaType;

                var formulaTypeVariables = await _db.FormulaTypeVariablesCollection.AsQueryable()
                    .Where(f => f.Type == formulaType)
                    .FirstOrDefaultAsync();

                if (formulaTypeVariables == null)
                {
                    formulaTypeVariables = FormulaVariables.Create(
                        formulaType, request.Variables
                    ).Data;

                    await _db.FormulaTypeVariablesCollection.InsertOneAsync(
                        formulaTypeVariables, cancellationToken: cancellationToken
                    );
                }
                else
                {
                    formulaTypeVariables.Variables = request.Variables;

                    await _db.FormulaTypeVariablesCollection.ReplaceOneAsync(
                        ft => ft.Type == formulaType, formulaTypeVariables,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }
        }
    }
}
