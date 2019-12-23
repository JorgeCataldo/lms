using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ChangeEventUserApplicationStatusCommand
    {
        public class Contract : CommandContract<Result>{
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string UserId { get; set; }
            public int ApplicationStatus { get; set; }
            public string ManagerUserId { get; set; }
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
                    return Result.Fail("Acesso Negado");

                var eventId = ObjectId.Parse(request.EventId);
                var scheduleId = ObjectId.Parse(request.EventScheduleId);
                var userId = ObjectId.Parse(request.UserId);
                var managerUserId = ObjectId.Parse(request.ManagerUserId);

                if (request.UserRole == "Student")
                {
                    var instructorTutor = await _db.EventCollection.AsQueryable()
                        .FirstOrDefaultAsync(x =>
                        x.Id == eventId &&
                        x.InstructorId == managerUserId ||
                        x.TutorsIds.Contains(managerUserId)
                    );

                    if (instructorTutor == null)
                    {
                        
                        var responsible = await _db.ResponsibleCollection.AsQueryable()
                            .FirstOrDefaultAsync(x => x.ResponsibleUserId == managerUserId);

                        if (responsible == null)
                            return Result.Fail<Contract>("Acesso Negado");
                    }
                }

                var query = await _db.EventApplicationCollection.FindAsync(
                    u => u.EventId == eventId &&
                    u.UserId == userId &&
                    u.ScheduleId == scheduleId,
                    cancellationToken: cancellationToken
                );

                var eventApplication = await query.SingleOrDefaultAsync(cancellationToken);
                if (eventApplication == null)
                    return Result.Fail<Contract>("Inscrição não existe");

                eventApplication.ApplicationStatus = (ApplicationStatus)request.ApplicationStatus;

                await _db.EventApplicationCollection.ReplaceOneAsync(t =>
                    t.Id == eventApplication.Id, eventApplication,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }

            private async Task<Event> GetEvent(ObjectId eventId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Event>("Event")
                    .FindAsync(x => x.Id == eventId,
                        cancellationToken: token
                    );

                return await query.FirstOrDefaultAsync(token);
            }
        }
    }
}
