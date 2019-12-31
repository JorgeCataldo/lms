using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Activations.Commands
{
    public class CreateCustomActivationCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public bool Active { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var newAct = Activation.Create(request.Active, ActivationTypeEnum.Custom, request.Title, request.Text, request.Percentage);

                if (newAct.IsFailure)
                    return Result.Fail<bool>("Ocorreu um ma criação");

                await _db.ActivationsCollection.InsertOneAsync(newAct.Data);

                return Result.Ok(true);
            }
        }
    }
}
