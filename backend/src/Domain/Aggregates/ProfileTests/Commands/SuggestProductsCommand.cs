using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.SuggestedProduct;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class SuggestProductsCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string ResponseId { get; set; }
            public List<SuggestedProductItem> Products { get; set; }
            public string CurrentUserRole { get; set; }
        }

        public class SuggestedProductItem
        {
            public string ProductId { get; set; }
            public SuggestedProductType Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole == "Student" || request.CurrentUserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.ResponseId))
                    return Result.Fail("Acesso Negado");

                if (request.Products == null || request.Products.Count == 0)
                    return Result.Fail("Pelo menos um produto precisa ser selecionado para recomendação");

                var response = await _db.ProfileTestResponseCollection.AsQueryable()
                    .Where(r => r.Id == ObjectId.Parse(request.ResponseId))
                    .FirstOrDefaultAsync();

                if (response == null)
                    return Result.Fail<Contract>("Resposta de questionário não existe");

                await _db.SuggestedProductCollection.DeleteManyAsync(
                    p => p.UserId == response.CreatedBy,
                    cancellationToken: cancellationToken
                );
                    
                var products = new List<SuggestedProduct>();

                foreach (var product in request.Products)
                {
                    var newProduct = Create(
                        ObjectId.Parse(product.ProductId),
                        product.Type,
                        response.CreatedBy
                    ).Data;

                    products.Add(newProduct);
                }

                await _db.SuggestedProductCollection.InsertManyAsync(
                    products, cancellationToken: cancellationToken
                );

                response.Recommended = true;

                await _db.ProfileTestResponseCollection.ReplaceOneAsync(
                    t => t.Id == response.Id, response,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
