using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class DeleteModuleDraftCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
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
                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var draftId = ObjectId.Parse(request.ModuleId);

                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == draftId || x.ModuleId == draftId
                        )
                    )
                    .FirstOrDefaultAsync();

                if (module == null)
                    return Result.Fail("Rascunho de Módulo não encontrado");


                if (module.CreatedBy != userId)
                    return Result.Fail("Você não tem permissão de excluir o módulo selecionado.");

                if (request.UserRole == "Student")
                {
                    if (!ModuleDraft.IsInstructor(module, userId).Data)
                        return Result.Fail("Acesso Negado");
                }

                module.DeletedBy = ObjectId.Parse(request.UserId);
                module.DeletedAt = DateTimeOffset.Now;

                await _db.ModuleDraftCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                var oldDraftList = new List<ModuleDraft>
                {
                    module
                };

                var changeLog = AuditLog.Create(userId, draftId, module.GetType().ToString(),
                    "", EntityAction.Delete, JsonConvert.SerializeObject(oldDraftList));

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                var questionDrafts = await _db.QuestionDraftCollection.AsQueryable()
                    .Where(d => d.ModuleId == module.ModuleId && !d.DraftPublished)
                    .ToListAsync();

                if (questionDrafts.Count > 0)
                {
                    var update = Builders<QuestionDraft>.Update
                        .Set(d => d.DeletedAt, DateTimeOffset.Now)
                        .Set(d => d.DeletedBy, ObjectId.Parse(request.UserId));

                    var ids = questionDrafts.Select(x => x.Id).ToArray();

                    await _db.QuestionDraftCollection.UpdateManyAsync(x =>
                        ids.Contains(x.Id), update,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }
        }
    }
}
