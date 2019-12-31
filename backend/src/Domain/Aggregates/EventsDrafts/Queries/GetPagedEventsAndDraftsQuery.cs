using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using Domain.Extensions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventsDrafts.Queries
{
    public class GetPagedEventsAndDraftsQuery
    {
        public class Contract : CommandContract<Result<PagedEventItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
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
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public bool Published { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
            public ObjectId? DeletedBy { get; set; }
            public List<ObjectId> TutorsIds { get; set; }

            public bool? IsDraft { get; set; } = false;
            public ObjectId? EventId { get; set; }
        }

        public class EventDraftItem
        {
            public ObjectId Id { get; set; }
            public ObjectId EventId { get; set; }
            public bool DraftPublished { get; set; }
            public ObjectId? InstructorId { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public bool Published { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
            public List<ObjectId> TutorsIds { get; set; }
        }

        public class RequirementItem
        {
            public ObjectId ModuleId { get; set; }
        }

        public class EventScheduleItem
        {
            public DateTimeOffset EventDate { get; set; }
            public ObjectId Id { get; set; }
            public DateTimeOffset SubscriptionStartDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
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

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedEventItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var query = await SetEventsQuery(request, cancellationToken);

                var selectQuery = query
                    .OrderBy(x => x.Title)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);

                var eventsList = await selectQuery.ToListAsync(cancellationToken);

                var drafts = await GetEventsDrafts(eventsList);

                foreach (var draft in drafts)
                {
                    eventsList.RemoveAll(m => m.Id == draft.EventId);

                    eventsList.Add(
                        new EventItem
                        {
                            Id = draft.Id,
                            InstructorId = draft.InstructorId,
                            Title = draft.Title,
                            Published = draft.Published,
                            Excerpt = draft.Excerpt,
                            ImageUrl = draft.ImageUrl,
                            TutorsIds = draft.TutorsIds,
                            IsDraft = true
                        }
                    );
                }

                var result = new PagedEventItems()
                {
                    Page = request.Page,
                    ItemsCount = await query.CountAsync(),
                    Events = eventsList.OrderBy(x => x.Title).ToList()
                };

                return Result.Ok(result);
            }

            private async Task<IMongoQueryable<EventItem>> SetEventsQuery(Contract request, CancellationToken token)
            {
                var collection = _db.Database.GetCollection<EventItem>("Events");
                var query = collection.AsQueryable();

                query = query.Where(x =>
                    x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                );

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var user = await GetUserById(userId, token);

                    query = query.Where(x =>
                        x.InstructorId == userId ||
                        x.TutorsIds.Contains(userId)
                    );
                }

                if (request.Filters != null)
                {
                    if (!String.IsNullOrEmpty(request.Filters.Term))
                    {
                        request.Filters.Term = request.Filters.Term.ToLower().RemoveDiacritics();
                        query = query.Where(x =>
                            x.Title.ToLower().Contains(request.Filters.Term) ||
                            x.Excerpt.ToLower().Contains(request.Filters.Term)
                        );
                    }
                }

                return query;
            }

            private async Task<List<EventDraftItem>> GetEventsDrafts(List<EventItem> modules)
            {
                var modulesIds = modules.Select(m => m.Id);

                var draftsCollection = _db.Database.GetCollection<EventDraftItem>("EventsDrafts");
                return await draftsCollection
                    .AsQueryable()
                    .Where(draft =>
                        modulesIds.Contains(draft.EventId) &&
                        !draft.DraftPublished && (
                            draft.DeletedAt == null || draft.DeletedAt == DateTimeOffset.MinValue
                        )
                    )
                    .ToListAsync();
            }

            private async Task<User> GetUserById(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(
                        x => x.Id == userId,
                        cancellationToken: token
                     );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }
        }
    }
}
