using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.QuestionsDraft.Commands
{
    public class RemoveQuestionDraft
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary")
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var questionId = ObjectId.Parse(request.Id);

                var question = await _db.QuestionDraftCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == questionId);

                if (question == null)
                    return Result.Fail("Pergunta não encontrada");

                await _db.QuestionDraftCollection.DeleteOneAsync(x =>
                    x.Id == questionId,
                    cancellationToken: cancellationToken
                );

                var oldQuestionList = new List<QuestionDraft>
                {
                    question
                };
                var oldValues = JsonConvert.SerializeObject(oldQuestionList);

                var deleteLog = AuditLog.Create(userId, question.DraftId,
                    oldQuestionList[0].GetType().ToString(), "", EntityAction.Delete, oldValues);

                await _db.AuditLogCollection.InsertOneAsync(deleteLog);

                return Result.Ok();
            }
        }
    }
}
