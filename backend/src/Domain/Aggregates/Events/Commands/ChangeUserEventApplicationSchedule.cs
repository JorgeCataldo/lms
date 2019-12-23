using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ChangeUserEventApplicationSchedule
    {
        
        public class Contract : CommandContract<Result<bool>>
        {
            public string UserId { get; set; }
            public string EventId { get; set; }
            public string ScheduleId { get; set; }
            public string NewScheduleId { get; set; }
            public string CurrentUserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var uId = ObjectId.Parse(request.UserId);
                var eId = ObjectId.Parse(request.EventId);
                var sId = ObjectId.Parse(request.ScheduleId);
                var nsId = ObjectId.Parse(request.NewScheduleId);

                var evt = await (await _db
                    .Database
                    .GetCollection<EventApplication>("EventApplications")
                    .FindAsync(
                        x => x.UserId == uId &&
                        x.EventId == eId &&
                        x.ScheduleId == sId,
                        cancellationToken: cancellationToken
                    ))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return Result.Fail<bool>("Evento não Encontrado");

                evt.ScheduleId = nsId;

                await _db.EventApplicationCollection.ReplaceOneAsync(t =>
                    t.Id == evt.Id, evt,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(true);
            }
        }
    }
}
