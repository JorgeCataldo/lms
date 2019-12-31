using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums.Queries
{
    public class GetEventForumByEventScheduleQuery
    {
        public class Contract : CommandContract<Result<PagedForumItem>>
        {
            public string EventScheduleId { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserId { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
        }

        public class ForumItem
        {
            public ObjectId Id { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public string EventName { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class PagedForumItem
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public string EventName { get; set; }
            public long ItemsCount { get; set; }
            public bool IsInstructor { get; set; }
            public List<ForumQuestionItem> Questions { get; set; }
            public List<ForumQuestionItem> LastQuestions { get; set; }
        }

        public class ForumQuestionItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool Liked { get; set; }
            public List<string> LikedBy { get; set; }
            public List<EventForumAnswer> Answers { get; set; } = new List<EventForumAnswer>();
            public DateTimeOffset CreatedAt { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedForumItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedForumItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.EventScheduleId))
                        return Result.Fail<PagedForumItem>("Id do Evento não informado");

                    var forum = await GetForum(request.EventScheduleId, cancellationToken);

                    if (forum == null)
                    {
                        var newForum = await CreateForum(request.EventScheduleId);

                        if (newForum.IsFailure)
                            return Result.Fail<PagedForumItem>(newForum.Error);
                        
                        return Result.Ok(
                            new PagedForumItem() {
                                EventId = newForum.Data.EventId.ToString(),
                                EventScheduleId = newForum.Data.EventScheduleId.ToString(),
                                EventName = newForum.Data.EventName,
                                ItemsCount = 0,
                                Questions = new List<ForumQuestionItem>()
                            }
                        );
                    }
                    else
                    {
                        var pagedQuestions = await GetQuestions(
                            forum,
                            request,
                            cancellationToken
                        );
                        return Result.Ok(pagedQuestions);
                    }
                }
                catch (Exception err)
                {
                    return Result.Fail<PagedForumItem>(err.Message);
                }
            }

            private async Task<ForumItem> GetForum(string sEventScheduleId, CancellationToken cancellationToken)
            {
                var eventScheduleId = ObjectId.Parse(sEventScheduleId);
                var query = await _db.Database
                    .GetCollection<ForumItem>("EventForums")
                    .FindAsync(x => x.EventScheduleId == eventScheduleId);

                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            }

            private async Task<Result<EventForum>> CreateForum(string sEventScheduleId)
            {
                var esId = ObjectId.Parse(sEventScheduleId);

                var dbEvent = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => x.Schedules.Any(y => y.Id == esId))
                    .FirstOrDefaultAsync();

                var newEventForum = EventForum.Create(
                    dbEvent.Id,
                    esId,
                    dbEvent.Title,
                    new List<ObjectId>()
                );

                await _db.EventForumCollection.InsertOneAsync(newEventForum.Data);

                return newEventForum;
            }

            private async Task<PagedForumItem> GetQuestions(ForumItem forum, Contract request, CancellationToken cancellationToken)
            {
                var options = new FindOptions<ForumQuestionItem>() {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<ForumQuestionItem> filters = SetFilters(request);
                var collection = _db.Database.GetCollection<ForumQuestionItem>("EventForumQuestions");

                var query = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var queryLast = await collection.FindAsync(filters,
                    options: new FindOptions<ForumQuestionItem>(),
                    cancellationToken: cancellationToken
                );

                var questions = await query.ToListAsync(cancellationToken);

                foreach (var question in questions)
                {
                    question.Liked = question.LikedBy.Any(uId => uId == request.UserId);
                    question.Answers = await (await _db.Database
                    .GetCollection<EventForumAnswer>("EventForumAnswers")
                    .FindAsync(x => x.QuestionId == question.Id))
                    .ToListAsync();
                }
                
                var lastQuestions = await queryLast.ToListAsync(cancellationToken);

                foreach (var lastQuestion in lastQuestions)
                {
                    lastQuestion.Liked = lastQuestion.LikedBy.Any(uId => uId == request.UserId);
                    lastQuestion.Answers = await (await _db.Database
                    .GetCollection<EventForumAnswer>("EventForumAnswers")
                    .FindAsync(x => x.QuestionId == lastQuestion.Id))
                    .ToListAsync();
                }

                lastQuestions = lastQuestions.OrderByDescending(x => x.CreatedAt).Take(4).ToList();

                return new PagedForumItem() {
                    EventId = forum.EventId.ToString(),
                    EventScheduleId = forum.EventScheduleId.ToString(),
                    EventName = forum.EventName,
                    IsInstructor = await CheckEventInstructor(
                        ObjectId.Parse(request.EventScheduleId),
                        ObjectId.Parse(request.UserId),
                        cancellationToken
                    ),
                    ItemsCount = await collection.CountDocumentsAsync(filters),
                    Questions = questions,
                    LastQuestions = lastQuestions
                };
            }

            private FilterDefinition<ForumQuestionItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<ForumQuestionItem>.Empty;
                var builder = Builders<ForumQuestionItem>.Filter;

                filters = filters & builder.Eq("eventScheduleId", ObjectId.Parse(request.EventScheduleId));

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                {
                    filters = filters &
                        (GetRegexFilter(builder, "title", request.Filters.Term) |
                         GetRegexFilter(builder, "description", request.Filters.Term));
                }
                    
                return filters;
            }

            private FilterDefinition<ForumQuestionItem> GetRegexFilter(
                FilterDefinitionBuilder<ForumQuestionItem> builder, string prop, string term
            ) {
                return builder.Regex(prop,
                    new BsonRegularExpression("/" + term + "/is")
                );
            }

            private async Task<bool> CheckEventInstructor(
                ObjectId eventScheduleId, ObjectId userId, CancellationToken cancellationToken
            ) {
                var query = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x =>
                        x.Schedules.Any(y => y.Id == eventScheduleId) &&
                        x.InstructorId.HasValue &&
                        x.InstructorId.Value == userId,
                        cancellationToken: cancellationToken
                    );
                return await query.AnyAsync();
            }
        }
    }
}
