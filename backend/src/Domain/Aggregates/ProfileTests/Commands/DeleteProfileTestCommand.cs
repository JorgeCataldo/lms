using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class DeleteProfileTestCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string UserRole { get; set; }
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
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.Id))
                    return Result.Fail("Id do Teste não Informado");

                await _db.ProfileTestCollection.DeleteOneAsync(
                    t => t.Id == ObjectId.Parse(request.Id),
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
