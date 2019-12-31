using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventsDrafts.Commands
{
    public class DeleteEventDraft
    {
        public class Contract : CommandContract<Result>
        {
            public string EventId { get; set; }
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

                var draftId = ObjectId.Parse(request.EventId);

                var draft = await _db.EventDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == draftId || x.EventId == draftId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (draft == null)
                    return Result.Fail("Rascunho não encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!draft.InstructorId.HasValue || draft.InstructorId.Value != userId)
                        return Result.Fail("Acesso Negado");
                }

                draft.DeletedBy = ObjectId.Parse(request.UserId);
                draft.DeletedAt = DateTimeOffset.Now;

                await _db.EventDraftCollection.ReplaceOneAsync(t =>
                    t.Id == draft.Id, draft,
                    cancellationToken: cancellationToken
                );
                
                var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), draftId, draft.GetType().ToString(),
                    "", EntityAction.Delete, JsonConvert.SerializeObject(draft));


                return Result.Ok();
            }
        }
    }
}
