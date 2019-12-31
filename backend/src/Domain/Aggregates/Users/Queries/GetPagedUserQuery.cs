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

namespace Domain.Aggregates.Users.Queries
{
    public class GetPagedUserQuery
    {
        public class Contract : CommandContract<Result<PagedUserItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
            public ObjectId UserId { get; set; }
        }

        public class RequestFilters
        {
            public string Name { get; set; }
            public bool? Blocked { get; set; }
            public string SortBy { get; set; }
            public bool IsSortAscending { get; set; }
            public List<UserCategoryFilter> CategoryFilter { get; set; }
            public string Dependent { get; set; }
        }

        public class UserCategoryFilter
        {
            public string ColumnName { get; set; }
            public List<string> ContentNames { get; set; }
        }

        public class PagedUserItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public long BlockedItemsCount { get; set; }
            public List<UserItem> UserItems { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string UserName { get; set; }
            public string RegistrationId { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string LineManager { get; set; }
            public bool IsBlocked { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public RelationalItem Rank { get; set; }
            public string Email { get; set; }
        }

        public class RelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedUserItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedUserItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var options = new FindOptions<UserItem>()
                    {
                        Limit = request.PageSize,
                        Skip = (request.Page - 1) * request.PageSize
                    };

                    FilterDefinition<UserItem> filters = SetFilters(request);

                    if (!String.IsNullOrEmpty(request.Filters.SortBy))
                    {
                        if (request.Filters.IsSortAscending)
                        {
                            options.Sort = Builders<UserItem>.Sort.Ascending(request.Filters.SortBy);
                        }
                        else
                        {
                            options.Sort = Builders<UserItem>.Sort.Descending(request.Filters.SortBy);
                        }
                    }

                    var collection = _db.Database.GetCollection<UserItem>("Users");
                    var qry = await collection.FindAsync(filters,
                        options: options,
                        cancellationToken: cancellationToken
                    );

                    var blockedCount = await _db.UserCollection
                        .AsQueryable()
                        .CountAsync(x => x.IsBlocked);

                    var userItems = await qry.ToListAsync(cancellationToken);

                    var result = new PagedUserItems()
                    {
                        Page = request.Page,
                        ItemsCount = await collection.CountDocumentsAsync(filters, null, cancellationToken: cancellationToken),
                        UserItems = userItems,
                        BlockedItemsCount = blockedCount
                    };

                    return Result.Ok(result);

                }
                catch (Exception err)
                {
                    return Result.Fail<PagedUserItems>($"Ocorreu um erro ao buscar os usuarios: {err.Message}");
                }
            }

            private FilterDefinition<UserItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<UserItem>.Empty;
                var builder = Builders<UserItem>.Filter;

                filters = filters & builder.Ne(s => s.Id, request.UserId);

                if (request.UserRole == "Student")
                {
                    filters = filters & (
                        builder.Eq(s => s.Id, request.UserId) |
                        builder.Eq(s => s.ResponsibleId, request.UserId)
                    );
                }

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Name))
                {
                    filters = filters & builder.Where(x => x.Name.Contains(request.Filters.Name) || x.Email.Contains(request.Filters.Name));
                }

                if (request.Filters.Blocked.HasValue)
                {
                    filters = filters & builder.Where(x => x.IsBlocked == request.Filters.Blocked.Value);
                }

                if (request.UserRole == "Student" && !String.IsNullOrEmpty(request.Filters.Dependent))
                {
                    filters = filters & builder.Regex("lineManager", new BsonRegularExpression("/" + request.Filters.Dependent + "/is"));
                }

                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    foreach (UserCategoryFilter userCategoryFilter in request.Filters.CategoryFilter)
                    {
                        foreach (string contentName in userCategoryFilter.ContentNames)
                        {
                            filters = filters & builder.Regex(userCategoryFilter.ColumnName, new BsonRegularExpression("/" + contentName + "/is"));
                        }
                    }
                }

                return filters;
            }
        }
    }
}
