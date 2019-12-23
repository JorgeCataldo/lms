using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Queries
{
    public class GetPastEventsQuery
    {
        public class Contract : CommandContract<Result<ListResult>>
        {
            public string SearchTerm { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string UserRole { get; set; }
        }

        public class ListResult
        {
            public List<EventItem> Items { get; set; }
            public int ItemsCount { get; set; }
        }

        public class EventItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<EventSchedule> Schedules { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ListResult>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<ListResult>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var queryable = _db.Database.GetCollection<EventItem>("Events").AsQueryable();

                if (!String.IsNullOrEmpty(request.SearchTerm))
                {
                    queryable = queryable.Where(ev =>
                        ev.Title.ToLower().Contains(
                            request.SearchTerm.ToLower()
                        )
                    );
                }

                queryable = queryable
                    .Where(x =>
                        x.Schedules != null &&
                        x.Schedules.Any(sch =>
                            sch.EventDate < DateTimeOffset.Now
                        ) && (
                            !x.DeletedAt.HasValue ||
                            x.DeletedAt.Value == DateTimeOffset.MinValue
                        )
                    );

                var eventsDb = await queryable
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Result.Ok(
                    new ListResult {
                        Items = eventsDb,
                        ItemsCount = await queryable.CountAsync()
                    }
                );
            }
        }
    }
}
