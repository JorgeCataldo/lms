using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Enumerations;
using MediatR;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;
using System.Linq;
using MongoDB.Driver;
using Domain.Base;

namespace Domain.Aggregates.Modules.Commands
{
    public class RemoveActionCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Description { get; set; }
            public int TypeId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
            public string UserId { get; set; }
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
                try
                {
                    var typeResult = ActionType.Create(request.TypeId);

                    await _db.ActionCollection.DeleteManyAsync(x =>
                        x.ModuleId == request.ModuleId &&
                        x.SubjectId == request.SubjectId &&
                        x.ContentId == request.ContentId &&
                        x.Type == typeResult.Data &&
                        x.CreatedBy == ObjectId.Parse(request.UserId),
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw err;
                }
            }
        }
    }
}
