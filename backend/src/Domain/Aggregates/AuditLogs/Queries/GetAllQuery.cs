using Domain.Data;
using Domain.Aggregates.Questions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tg4.Infrastructure.Functional;
using Domain.Extensions;
using Domain.Aggregates.ModulesDrafts;
using Performance.Domain.Aggregates.AuditLogs;
using System.Linq;

namespace Domain.Aggregates.AuditLogs.Queries
{
    public class GetAllQuery
    {
        public class Contract : IRequest<Result<List<ExportItem>>>
        {
            public string CurrentUserRole { get; set; }
        }

        public class ExportItem
        {
            public ObjectId Id { get; set; }
            public ObjectId EntityId { get; set; }
            public string OldValues { get; set; }
            public string NewValues { get; set; }
            public string Action { get; set; }
            public string ActionDescription { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ExportItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ExportItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Admin" && request.CurrentUserRole != "HumanResources" && request.CurrentUserRole != "Recruiter")
                    return Result.Fail<List<ExportItem>>("Acesso Negado");

                var auditLogs = await _db.AuditLogCollection
                    .AsQueryable()
                    .OrderBy(x => x.Date)
                    .ToListAsync(cancellationToken: cancellationToken);

                if (auditLogs == null || auditLogs.Count == 0)
                    return Result.Fail<List<ExportItem>>("Logs não encontrados");

                var exportItems = new List<ExportItem>();

                for (var i = 0; i < auditLogs.Count; i++)
                {
                    var currentLog = auditLogs[i];
                    var exportItem = new ExportItem
                    {
                        Id = currentLog.Id,
                        EntityId = currentLog.EntityId,
                        OldValues = "",
                        NewValues = "",
                        Action = currentLog.Action.ToString(),
                        ActionDescription = Enum.GetName(typeof(EntityAction), currentLog.Action)
                    };

                    if (currentLog.Action == EntityAction.Update)
                    {
                        var variances = GetVariances(currentLog.EntityType, currentLog.OldValues, currentLog.NewValues);
                    }
                    else if (currentLog.Action == EntityAction.Add)
                    {
                        exportItem.NewValues = currentLog.NewValues;
                    }
                    else
                    {
                        exportItem.OldValues = currentLog.OldValues;
                    }
                }

                return Result.Ok(exportItems);
            }

            private List<Variance> GetVariances (string type, string oldValues, string newValues)
            {
                var variances = new List<Variance>();
                switch (type)
                {
                    case "Domain.Aggregates.Questions.QuestionDraft":
                        var parsedOldQuestions = !string.IsNullOrEmpty(oldValues)
                        ? JsonConvert.DeserializeObject<List<QuestionDraft>>(oldValues)
                        : new List<QuestionDraft>();

                        var parsedNewQuestions = !string.IsNullOrEmpty(newValues)
                        ? JsonConvert.DeserializeObject<List<QuestionDraft>>(newValues)
                        : new List<QuestionDraft>();
                        
                        return parsedOldQuestions.First().DetailedCompare(parsedNewQuestions.First());
                    
                    case "Domain.Aggregates.ModulesDrafts.ModuleDraft":
                        var parsedOldModule = !string.IsNullOrEmpty(oldValues)
                        ? JsonConvert.DeserializeObject<ModuleDraft>(oldValues)
                        : null;

                        var parsedNewModule= !string.IsNullOrEmpty(newValues)
                        ? JsonConvert.DeserializeObject<ModuleDraft>(newValues)
                        : null;

                        return parsedOldModule.DetailedCompare(parsedNewModule);
                    default:
                        return new List<Variance>();
                }
                
            }
        }
    }
}
