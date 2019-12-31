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
    public class ManageQuestionLikeCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string QuestionId { get; set; }
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
                    if (String.IsNullOrEmpty(request.QuestionId))
                        return Result.Fail("Id da pergunta não informado");

                    var question = await GetQuestion(request.QuestionId, cancellationToken);
                    if (question == null)
                        return Result.Fail("Pergunta não existe");
                    
                    if (request.Liked && !IsQuestionLikedByUser(question, request.UserId))
                    {
                        question.LikedBy.Add(request.UserId);
                    }
                    else if (!request.Liked)
                    {
                        question.LikedBy = GetLikedByWithoutUser(question, request.UserId);
                    }

                    await _db.ForumQuestionCollection.ReplaceOneAsync(fQ =>
                        fQ.Id == question.Id, question,
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

            private async Task<ForumQuestion> GetQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);

                var query = await _db.Database
                    .GetCollection<ForumQuestion>("ForumQuestions")
                    .FindAsync(
                        x => x.Id == questionId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private bool IsQuestionLikedByUser(ForumQuestion question, string userId)
            {
                return question.LikedBy.Any(uId => uId == userId);
            }

            private List<string> GetLikedByWithoutUser(ForumQuestion question, string userId)
            {
                return question.LikedBy
                    .Where(uId => uId != userId)
                    .ToList();
            }
        }
    }
}
