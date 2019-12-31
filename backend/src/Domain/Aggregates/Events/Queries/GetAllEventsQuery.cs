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
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Events.Queries
{
    public class GetAllEventsByUserQuery
    {
        public class Contract : CommandContract<Result<List<EventItem>>>
        {
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public List<UserProgressItem> ModulesInfo { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<UserProgressItem> EventsInfo { get; set; }
        }
        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public bool Blocked { get; set; }
        }

        public class TrackInfo
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackEvent> EventsConfiguration { get; set; }
        }

        public class EventItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<EventItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<EventItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userId = ObjectId.Parse(request.UserId);
                var user = await _db.Database
                    .GetCollection<UserItem>("Users")
                    .AsQueryable()
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync();

                var trackIds = user.TracksInfo.Select(x => x.Id).ToList();

                var tracks = await _db.Database
                .GetCollection<TrackInfo>("Tracks")
                .AsQueryable()
                .Where(x => trackIds.Contains(x.Id))
                .ToListAsync();

                var events = new List<EventItem>();

                foreach (var track in tracks)
                {
                    var trackInfo = track.EventsConfiguration.Select(x => new EventItem {Id = x.EventId, Title = x.Title });
                    events.AddRange(trackInfo);
                }

                return Result.Ok(events);
            }
        }        
    }
}
