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
    public class UpdateCustomActivationCommand
    {
        public class Contract : CommandContract<Result<Activation>>
        {
            public string ActivationId { get; set; }
            public bool Active { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Activation>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<Activation>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var actId = ObjectId.Parse(request.ActivationId);

                var act = await _db.ActivationsCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == actId);

                if (act == null)
                    return Result.Fail<Activation>("Objeto não encontrado");

                act.Active = request.Active;
                act.Title = request.Title;
                act.Text = request.Text;
                act.Percentage = request.Percentage;

                await _db.ActivationsCollection.ReplaceOneAsync(t =>
                    t.Id == actId, act,
                    cancellationToken: cancellationToken
                );
                
                return Result.Ok(act);
            }
        }
    }
}
