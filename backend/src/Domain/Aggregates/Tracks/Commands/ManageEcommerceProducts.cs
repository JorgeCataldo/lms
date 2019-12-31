using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class ManageEcommerceProducts
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string UserRole { get; set; }
            public string TrackId { get; set; }
            public List<EcommerceProductItem> Products { get; set; }
        }

        public class EcommerceProductItem
        {
            public long EcommerceId { get; set; }
            public int UsersAmount { get; set; }
            public bool DisableEcommerce { get; set; }
            public string Price { get; set; }
            public bool DisableFreeTrial { get; set; }
            public string LinkEcommerce { get; set; }
            public string LinkProduct { get; set; }
            public string Subject { get; set; }
            public string Hours { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<Contract>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await _db.TrackCollection.AsQueryable()
                    .Where(t => t.Id == trackId)
                    .FirstOrDefaultAsync();

                if (track == null)
                    return Result.Fail<Contract>("Trilha não existe");

                var products = new List<EcommerceProduct>();
                foreach (var product in request.Products)
                {
                    var ecommerceProduct = EcommerceProduct.Create(
                        product.EcommerceId,
                        product.UsersAmount,
                        product.DisableEcommerce,
                        product.Price,
                        product.DisableFreeTrial,
                        product.LinkEcommerce,
                        product.LinkProduct,
                        product.Subject,
                        product.Hours
                    );

                    products.Add(ecommerceProduct.Data);
                }

                track.EcommerceProducts = products;

                await _db.TrackCollection.ReplaceOneAsync(t =>
                    t.Id == track.Id, track,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
