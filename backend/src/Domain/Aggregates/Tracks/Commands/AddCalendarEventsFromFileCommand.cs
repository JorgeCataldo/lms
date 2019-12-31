using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class AddCalendarEventsFromFileCommand
    {
        public class Contract : CommandContract<Result<List<CalendarEvent>>>
        {
            public string TrackId { get; set; }
            public bool ConcatEvents { get; set; } = false;
            public string FileContent { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<CalendarEvent>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<List<CalendarEvent>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var trackId = ObjectId.Parse(request.TrackId);

                var track = _db.TrackCollection.AsQueryable()
                    .FirstOrDefault(x => x.Id == trackId);

                if (track == null)
                    return Result.Fail<List<CalendarEvent>>("Track não encontrada");

                var list = ReadAsList(request.FileContent);

                if (list == null)
                    return Result.Fail<List<CalendarEvent>>("Lista de eventos vazia");

                List<CalendarEvent> calendarEventsFile = new List<CalendarEvent>();

                for (int i = 1; i < list.Count - 1; i++)
                {
                    var splittedRow = list[i].Split(',');
                    calendarEventsFile.Add(ParseToCalendarEvent(splittedRow));
                }

                if (request.ConcatEvents)
                {
                    track.CalendarEvents.AddRange(calendarEventsFile);
                }
                else
                {
                    track.CalendarEvents = calendarEventsFile;
                }

                await _db.TrackCollection.ReplaceOneAsync(t =>
                    t.Id == track.Id, track,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(calendarEventsFile);
            }

            private List<string> ReadAsList(string content)
            {
                return string.IsNullOrEmpty(content)
                    ? null
                    : content.Split('\n').ToList();
            }

            private CalendarEvent ParseToCalendarEvent(string[] calendarEventFile)
            {
                var calendarEventItem = new CalendarEvent
                {
                    Title = calendarEventFile[0]
                };

                var dateSplit = calendarEventFile[1].Split('/');
                calendarEventItem.EventDate = new DateTimeOffset(int.Parse(dateSplit[2]), int.Parse(dateSplit[1]), int.Parse(dateSplit[0]), 0, 0, 0, TimeSpan.Zero);

                if (string.IsNullOrEmpty(calendarEventFile[2]))
                {
                    calendarEventItem.Duration = null;
                }
                else {
                    calendarEventItem.Duration = int.Parse(calendarEventFile[2]);
                }
                return calendarEventItem;
            }
        }
    }
}
