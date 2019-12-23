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
    public class DeleteFormula
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public string FormuladId { get; set; }
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

                if (string.IsNullOrEmpty(request.FormuladId))
                    return Result.Fail<Result>("Acesso Negado");

                var formulaId = ObjectId.Parse(request.FormuladId);

                var formula = await _db.FormulaCollection.AsQueryable()
                    .Where(f => f.Id == formulaId)
                    .FirstOrDefaultAsync();

                if (formula == null)
                    return Result.Fail<Result>("Fórmula não existe");

                formula.DeletedAt = DateTimeOffset.Now;
                formula.DeletedBy = ObjectId.Parse(request.UserId);

                await _db.FormulaCollection.ReplaceOneAsync(
                    f => f.Id == formulaId, formula,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(formula);
            }
        }
    }
}
