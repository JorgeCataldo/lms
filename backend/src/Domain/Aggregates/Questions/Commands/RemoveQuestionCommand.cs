﻿using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Questions.Commands
{
    public class RemoveQuestionCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string Id { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract,
            Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                var mId = ObjectId.Parse(request.Id);
                var deleted = await _db.QuestionCollection.DeleteOneAsync(x =>
                    x.Id == mId,
                    cancellationToken: cancellationToken
                );

                return deleted.DeletedCount == 1 ?
                    Result.Ok(true) :
                    Result.Fail<bool>("Pergunta não encontrada");
            }
        }
    }
}
