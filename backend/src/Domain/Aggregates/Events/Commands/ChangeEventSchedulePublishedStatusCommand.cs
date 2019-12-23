using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ChangeEventSchedulePublishedStatusCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>{
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                var eventsQry = await _db.EventCollection.FindAsync(u => u.Id == ObjectId.Parse(request.EventId), cancellationToken: cancellationToken);
                var singleEvent = await eventsQry.SingleOrDefaultAsync(cancellationToken);

                if (singleEvent == null)
                    return Result.Fail("Evento não existe");

                var eventSchedule = singleEvent.Schedules.Find(x => x.Id == ObjectId.Parse(request.EventScheduleId));

                if (eventSchedule == null)
                    return Result.Fail("Agendamento em evento não existe");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!singleEvent.InstructorId.HasValue || singleEvent.InstructorId != userId)
                        return Result.Fail("Acesso Negado");
                }

                eventSchedule.Published = !eventSchedule.Published;

                await _db.EventCollection.ReplaceOneAsync(t =>
                    t.Id == singleEvent.Id, singleEvent,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
