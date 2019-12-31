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
    public class GetPagedEventsQuery
    {
        public class Contract : CommandContract<Result<PagedEventItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
            public bool WithSchedule { get; set; } = false;
        }

        public class PagedEventItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<EventItem> Events { get; set; }
        }

        public class EventItem
        {
            public ObjectId Id { get; set; }
            public ObjectId? InstructorId { get; set; }
            public string Title { get; set; }
            public string Excerpt {get;set; }
            public string ImageUrl { get; set; }
            public bool Published { get; set; }
            public EventScheduleItem NextSchedule { get; set; }
            public List<EventScheduleItem> Schedules { get; set; }
            public bool HasUserProgess { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
            public ObjectId? DeletedBy { get; set; }
            public List<RequirementItem> Requirements { get; set; }
            public List<ObjectId> TutorsIds { get; set; }

            public EventItem()
            {
                Schedules = new List<EventScheduleItem>();
            }
        }

        public class RequirementItem
        {
            public ObjectId ModuleId { get; set; }
        }

        public class EventScheduleItem
        {
            public DateTimeOffset EventDate {get;set;}
            public ObjectId Id { get; set; }
            public DateTimeOffset SubscriptionStartDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
            public DateTimeOffset? ForumStartDate { get; set; }
            public DateTimeOffset? ForumEndDate { get; set; }
            public int Duration { get; set; }
            public bool Published { get; set; }
        }

        public class UserEventItem
        {
            public ObjectId EventId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedEventItems>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<PagedEventItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var options = new FindOptions<EventItem>()
                    {
                        Sort = Builders<EventItem>.Sort.Ascending("Title"),
                        Limit = request.PageSize,
                        Skip = (request.Page - 1) * request.PageSize
                    };
                    
                    FilterDefinition<EventItem> filters = SetFilters(request);
                    var collection = _db.Database.GetCollection<EventItem>("Events");

                    var qry = await collection.FindAsync(filters,
                        options: options,
                        cancellationToken: cancellationToken
                    );

                    var eventAppCollection = _db.Database.GetCollection<UserEventItem>("EventApplications");
                    var eventAppQuery = eventAppCollection.AsQueryable();

                    var eventList = await qry.ToListAsync(cancellationToken);

                    if (request.Filters.WithSchedule)
                    {
                        foreach (var item in eventList)
                        {
                            item.NextSchedule = item.Schedules.FirstOrDefault(x =>
                                x.EventDate >= DateTimeOffset.Now
                            );
                        }
                    }

                    var result = new PagedEventItems()
                    {
                        Page = request.Page,
                        ItemsCount = await collection.CountDocumentsAsync(
                            filters,
                            cancellationToken: cancellationToken
                        ),
                        Events = eventList
                    };


                    return Result.Ok(result);
                }
                catch (Exception err)
                {
                    return Result.Fail<PagedEventItems>($"Ocorreu um erro ao buscar os eventos: {err.Message}");
                }
            }

            private FilterDefinition<EventItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<EventItem>.Empty;
                var builder = Builders<EventItem>.Filter;

                filters = filters & (
                    builder.Eq(x => x.DeletedAt, null) |
                    builder.Eq(x => x.DeletedAt, DateTimeOffset.MinValue)
                );

               if (request.UserRole == "Student")
               {
                    var userId = ObjectId.Parse(request.UserId);
                   filters = filters & (
                        builder.Eq(x => x.InstructorId, userId) |
                        builder.AnyEq(x => x.TutorsIds, userId)
                    );
               }

                if (request.UserRole == "BusinessManager")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var user = GetUserById(userId);
                    if (user != null)
                    {
                        var eventIds = user.EventsInfo.Select(x => x.Id);
                        filters = filters & builder.In(x => x.Id, eventIds);
                    }
                }

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                    filters = filters & builder.Regex("title",
                                  new BsonRegularExpression("/" + request.Filters.Term + "/is"));

                if (request.Filters.WithSchedule)
                    filters = filters & builder.SizeGt(x => x.Schedules, 0);

                return filters;
            }

            private User GetUserById(ObjectId userId)
            {
                return _db.UserCollection
                    .AsQueryable()
                    .FirstOrDefault(x => x.Id == userId);
            }
        }
    }
}
