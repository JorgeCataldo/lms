using System;
using System.Collections.Generic;
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
using static Domain.Aggregates.Formula;

namespace Domain.Aggregates.Formulas.Commands
{
    public class GetFormulas
    {
        public class Contract : CommandContract<Result<PagedFormulas>>
        {
            public string UserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        public class FormulaPartItem
        {
            public int Order { get; set; }
            public int? Operator { get; set; }
            public string Key { get; set; }
        }

        public class PagedFormulas
        {
            public long ItemsCount { get; set; }
            public List<FormulaItem> Formulas { get; set; }
        }

        public class FormulaItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public List<FormulaPart> FormulaParts { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedFormulas>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<PagedFormulas>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<PagedFormulas>("Acesso Negado");

                var formulasQuery = _db.FormulaCollection.AsQueryable()
                    .Where(f => f.DeletedAt == null || f.DeletedAt == DateTimeOffset.MinValue);

                var formulas = await formulasQuery
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(f => new FormulaItem() {
                        CreatedAt = f.CreatedAt,
                        FormulaParts = f.FormulaParts,
                        Id = f.Id,
                        Title = f.Title
                    })
                    .ToListAsync();

                var pagedFormulas = new PagedFormulas {
                    Formulas = formulas,
                    ItemsCount = await formulasQuery.CountAsync()
                };

                return Result.Ok( pagedFormulas );
            }
        }
    }
}
