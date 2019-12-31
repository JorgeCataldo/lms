using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class ManageTrackCalendarEventsCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string TrackId { get; set; }
            public List<CalendarEventItem> CalendarEvents { get; set; }
            public string UserRole { get; set; }
        }

        public class CalendarEventItem
        {
            public int? Duration { get; set; }
            public string Title { get; set; }
            public DateTimeOffset EventDate { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<Contract>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await _db.TrackCollection.AsQueryable()
                    .Where(t => t.Id == trackId)
                    .FirstOrDefaultAsync();

                if (track == null)
                    return Result.Fail<Contract>("Trilha não existe");

                var calEvents = new List<CalendarEvent>();
                foreach (var reqEvent in request.CalendarEvents)
                {
                    var calEvent = CalendarEvent.Create(
                        reqEvent.Title,
                        reqEvent.EventDate,
                        reqEvent.Duration
                    );

                    if (calEvent.IsSuccess)
                        calEvents.Add(calEvent.Data);
                }

                track.CalendarEvents = calEvents;

                await _db.TrackCollection.ReplaceOneAsync(t =>
                    t.Id == track.Id, track,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }
        }
    }
}
