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
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class SetDraftQuestionsLimit
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
            public int? QuestionsLimit { get; set; }
            public string UserId { get; set; }
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
                if (request.UserRole == "Secretary")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var moduleId = ObjectId.Parse(request.ModuleId);

                var draft = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == moduleId || x.ModuleId == moduleId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (draft == null)
                    return Result.Fail("Rascunho não Encontrado");

                var oldValues = JsonConvert.SerializeObject(new List<ModuleDraft>
                {
                    draft
                });

                draft.QuestionsLimit = request.QuestionsLimit;

                await _db.ModuleDraftCollection.ReplaceOneAsync(t =>
                    t.Id == draft.Id, draft,
                    cancellationToken: cancellationToken
                );

                var newDraftList = new List<ModuleDraft>
                {
                    draft
                };

                var changeLog = AuditLog.Create(userId, draft.Id, draft.GetType().ToString(),
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Update, oldValues);

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                return Result.Ok();
            }
        }
    }
}
