using Domain.Data;
using Domain.Aggregates.Questions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Performance.Domain.Aggregates.AuditLogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Questions.Queries.GetAllQuestionsQuery;

namespace Domain.Aggregates.AuditLogs.Queries
{
    public class GetUpdatedQuestionsDraftQuery
    {
        public class Contract : IRequest<Result<List<AuditLogItem>>>
        {
            public string CurrentUserRole { get; set; }
            public string ModuleId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string EntityType { get; set; }
        }

        public class ExportItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ModuleId { get; set; }
            public ObjectId QuestionId { get; set; }
            public string OldText { get; set; }
            public string NewText { get; set; }
            public int OldLevel { get; set; }
            public int NewLevel { get; set; }
            public int OldDuration { get; set; }
            public int NewDuration { get; set; }
        }

        public class AuditLogItem
        {
            public string OldValuePlain { get; set; }
            public string NewValuePlain { get; set; }

            public List<dynamic> OldValue { get; set; }
            public List<dynamic> NewValue { get; set; }

            public DateTimeOffset EventDate { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<AuditLogItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<AuditLogItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.CurrentUserRole != "Admin")
                        return Result.Fail<List<AuditLogItem>>("Acesso Negado");

                    if (String.IsNullOrEmpty(request.ModuleId))
                        return Result.Fail<List<AuditLogItem>>("Id do Módulo não informado");

                    var moduleId = ObjectId.Parse(request.ModuleId);

                    var values = await _db.Database.GetCollection<AuditLog>("AuditLogs")
                        .AsQueryable()
                        .Where(x => x.EntityId == moduleId && x.EntityType == request.EntityType)
                        .Select(x => new AuditLogItem
                        {
                            OldValuePlain = x.OldValues,
                            NewValuePlain = x.NewValues,
                            EventDate = x.Date
                        })
                        .OrderBy(x => x.EventDate)
                        .ToListAsync(cancellationToken: cancellationToken);

                    for (int i = 0; i < values.Count; i++)
                    {
                        values[i].NewValue = !string.IsNullOrEmpty(values[i].NewValuePlain)
                            ? JsonConvert.DeserializeObject<List<dynamic>>(values[i].NewValuePlain)
                            : null;
                        values[i].OldValue = !string.IsNullOrEmpty(values[i].OldValuePlain)
                            ? JsonConvert.DeserializeObject<List<dynamic>>(values[i].OldValuePlain)
                            : null;
                    }

                    return Result.Ok(values);
                }
                catch (Exception ex)
                {
                    return Result.Fail<List<AuditLogItem>>(ex.Message);
                }
            }
        }
    }
}
