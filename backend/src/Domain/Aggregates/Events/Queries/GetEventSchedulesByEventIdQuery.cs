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
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Queries
{
    public class GetEventSchedulesByEventIdQuery
    {
        public class Contract : CommandContract<Result<List<EventSchedule>>>
        {
            public string EventId { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<EventSchedule>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<EventSchedule>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var evtId = ObjectId.Parse(request.EventId);

                var evt = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => x.Id == evtId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (evt == null)
                    return Result.Fail<List<EventSchedule>>("Ocorreu um erro ao buscar o evento");

                return Result.Ok(evt.Schedules);
            }
        }
    }
}
