using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ExportEventUsersGradesTemplates
    {
        public class Contract : CommandContract<Result<List<UserEventApplicationItem>>>
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
        }

        public class UserEventApplicationItem
        {
            public ObjectId UserId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Event { get; set; }
            public string Group { get; set; }
            public string EventDate { get; set; }
            public List<BaseValue> GradeBaseValues { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<UserEventApplicationItem>>>
        {
            private readonly IDbContext _db;
            private readonly List<string> _userGrades = new List<string> { "Nota_QC", "Nota_TG", "Nota_FA", "Média_Autoavaliação", "Média_Nota_QC",
                "Média_Nota_TG", "Média_Nota_FA", "Média_Grupo", "Nota_Professor_Grupo", "Média_Final_Case", "Destaque_Justificativa" };

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<List<UserEventApplicationItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.EventId))
                    return Result.Fail<List<UserEventApplicationItem>>("Id do evento não informado");
                if (string.IsNullOrEmpty(request.EventScheduleId))
                    return Result.Fail<List<UserEventApplicationItem>>("Id do agendamento do evento não informado");
                
                var eventId = ObjectId.Parse(request.EventId);
                var eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                var dbEvent = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => x.Id == eventId)
                    .FirstOrDefaultAsync(cancellationToken);

                var schedule = dbEvent.Schedules.Where(x => x.Id == eventScheduleId).FirstOrDefault();

                var participantsIds = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => x.EventId == eventId && x.ScheduleId == eventScheduleId)
                    .Select(x => x.UserId)
                    .ToListAsync(cancellationToken);
                
                var participants = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => participantsIds.Contains(x.Id))
                    .Select(x => new UserEventApplicationItem
                    {
                        UserId = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        Event = dbEvent.Title,
                        EventDate = schedule.EventDate.ToString("dd/MM/yyyy")
                    })
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < participants.Count; i++)
                {
                    var gradeBaseValues = new List<BaseValue>();

                    for (int k = 0; k < _userGrades.Count; k++)
                    {
                        gradeBaseValues.Add(new BaseValue
                        {
                            Key = _userGrades[k],
                            Value = string.Empty
                        });
                    }

                    participants[i].GradeBaseValues = gradeBaseValues;
                }

                return Result.Ok(participants);
            }          
        }
    }
}
