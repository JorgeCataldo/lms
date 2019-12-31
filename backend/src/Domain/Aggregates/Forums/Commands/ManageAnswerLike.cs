using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Commands
{
    public class ManageAnswerLikeCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string AnswerId { get; set; }
            public string UserId { get; set; }
            public bool Liked { get; set; }
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
                    if (String.IsNullOrEmpty(request.AnswerId))
                        return Result.Fail("Id da resposta não informado");

                    var answer = await GetAnswer(request.AnswerId, cancellationToken);
                    if (answer == null)
                        return Result.Fail("Resposta não existe");

                    if (request.Liked && !IsQuestionLikedByUser(answer, request.UserId))
                    {
                        answer.LikedBy.Add(request.UserId);
                    }
                    else if (!request.Liked)
                    {
                        answer.LikedBy = GetLikedByWithoutUser(answer, request.UserId);
                    }

                    await _db.ForumAnswerCollection.ReplaceOneAsync(fA =>
                        fA.Id == answer.Id, answer,
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

            private async Task<ForumAnswer> GetAnswer(string rAnswerId, CancellationToken cancellationToken)
            {
                var answerId = ObjectId.Parse(rAnswerId);

                var query = await _db.Database
                    .GetCollection<ForumAnswer>("ForumAnswers")
                    .FindAsync(
                        x => x.Id == answerId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private bool IsQuestionLikedByUser(ForumAnswer answer, string userId)
            {
                return answer.LikedBy.Any(uId => uId == userId);
            }

            private List<string> GetLikedByWithoutUser(ForumAnswer answer, string userId)
            {
                return answer.LikedBy
                    .Where(uId => uId != userId)
                    .ToList();
            }
        }
    }
}
