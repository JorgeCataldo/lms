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
    public class GetFormulaTypeVariables
    {
        public class Contract : CommandContract<Result<List<FormulaVariablesItem>>>
        {
            public string UserRole { get; set; }
        }

        public class FormulaVariablesItem
        {
            public FormulaType Type { get; set; }
            public List<string> Variables { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<FormulaVariablesItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<List<FormulaVariablesItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<List<FormulaVariablesItem>>("Acesso Negado");

                var variables = await _db.FormulaTypeVariablesCollection
                    .AsQueryable()
                    .Select(v => new FormulaVariablesItem { Type = v.Type, Variables = v.Variables })
                    .ToListAsync();

                return Result.Ok(variables);
            }
        }
    }
}
