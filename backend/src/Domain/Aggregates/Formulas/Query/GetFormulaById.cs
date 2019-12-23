using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Formulas.Commands
{
    public class GetFormulaById
    {
        public class Contract : CommandContract<Result<Formula>>
        {
            public string UserRole { get; set; }
            public string FormulaId { get; set; }
        }

        public class FormulaPartItem
        {
            public int Order { get; set; }
            public int? Operator { get; set; }
            public string Key { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Formula>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<Formula>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<Formula>("Acesso Negado");

                if (String.IsNullOrEmpty(request.FormulaId))
                    return Result.Fail<Formula>("Acesso Negado");

                var formulaId = ObjectId.Parse(request.FormulaId);

                var formula = await _db.FormulaCollection.AsQueryable()
                    .Where(f => f.Id == formulaId)
                    .FirstOrDefaultAsync();

                if (formula == null)
                    return Result.Fail<Formula>("Fórmula não existe");

                return Result.Ok(formula);
            }
        }
    }
}
