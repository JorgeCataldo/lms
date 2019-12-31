using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Tracks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class DeleteEventCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string EventId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class UserEventItem
        {
            public ObjectId EventId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<bool>("Acesso Negado");

                var eId = ObjectId.Parse(request.EventId);
                var eventDb = await (await _db
                        .Database
                        .GetCollection<Event>("Events")
                        .FindAsync(x => x.Id == eId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                
                if (eventDb == null)
                    return Result.Fail<bool>("Módulo não Encontrado");

                eventDb.DeletedBy = ObjectId.Parse(request.UserId);
                eventDb.DeletedAt = DateTimeOffset.Now;

                var tracks = await _db.Database
                        .GetCollection<Track>("Tracks")
                        .AsQueryable()
                        .ToListAsync();

                foreach (Track track in tracks)
                {
                    int removed = track.EventsConfiguration.RemoveAll(x =>
                        x.EventId == eventDb.Id
                    );

                    if (removed > 0)
                    {
                        await _db.TrackCollection.ReplaceOneAsync(t =>
                            t.Id == track.Id, track,
                            cancellationToken: cancellationToken
                        );
                    }
                }

                var eventAppCollection = _db.Database.GetCollection<UserEventItem>("EventApplications");
                var eventAppQuery = eventAppCollection.AsQueryable();
                bool deleteEvent = eventAppQuery.FirstOrDefault(x => x.EventId == eventDb.Id) == null;

                var oldEventList = new List<Event>
                {
                    eventDb
                };
                

                if (deleteEvent)
                {
                    await _db.EventCollection.DeleteOneAsync(t =>
                        t.Id == eventDb.Id,
                        cancellationToken: cancellationToken
                    );

                }
                else
                {
                    await _db.EventCollection.ReplaceOneAsync(t =>
                        t.Id == eventDb.Id,
                        eventDb,
                        cancellationToken: cancellationToken
                    );
                }


                var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), eventDb.Id, eventDb.GetType().ToString(),
                "", EntityAction.Delete, JsonConvert.SerializeObject(oldEventList));

                await _db.AuditLogCollection.InsertOneAsync(changeLog);

                return Result.Ok(true);
            }
        }
    }
}
