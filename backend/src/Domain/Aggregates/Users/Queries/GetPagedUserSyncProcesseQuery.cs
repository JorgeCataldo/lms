using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetPagedUserSyncProcesseQuery
    {
        public class Contract : CommandContract<Result<PagedUserSyncProcesseItems>>
        {
            public string CurrentUserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public double? FromDate { get; set; }
            public double? ToDate { get; set; }
            public string SortBy { get; set; }
            public bool IsSortAscending { get; set; }
        }
        
        public class PagedUserSyncProcesseItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<UserSyncProcesseItem> UserSyncProcesseItems { get; set; }
        }

        public class UserSyncProcesseItem
        {
            public ObjectId Id { get; set; }
            public int ImportStatus { get; set; }
            public List<ImportError> ImportErrors { get; set; }
            public int TotalUsers { get; set; }
            public ImportedUsers NewUsers { get; set; }
            public ImportedUsers UpdatedUsers { get; set; }
            public ImportedUsers BlockedUsers { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        public class ImportError
        {
            public string ImportAction { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public long? Cge { get; set; }
            public string ImportErrorString { get; set; }
        }

        public class ImportedUsers
        {
            public List<ImportUser> Users { get; set; }
            public int Quantity { get; set; }
        }

        public class ImportUser
        {
            public string ImgUrl { get; set; }
            public string Name { get; set; }
            public string Rank { get; set; }
            public string Responsible { get; set; }
            public string Status { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedUserSyncProcesseItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedUserSyncProcesseItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Admin" && request.CurrentUserRole == "HumanResources" && request.CurrentUserRole == "Secretary")
                    return Result.Fail<PagedUserSyncProcesseItems>("Acesso Negado");

                var options = new FindOptions<UserSyncProcesseItem>()
                {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<UserSyncProcesseItem> filters = SetFilters(request);

                if (!String.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.IsSortAscending)
                    {
                        options.Sort = Builders<UserSyncProcesseItem>.Sort.Ascending(request.Filters.SortBy);
                    }
                    else
                    {
                        options.Sort = Builders<UserSyncProcesseItem>.Sort.Descending(request.Filters.SortBy);
                    }
                }

                var collection = _db.Database.GetCollection<UserSyncProcesseItem>("Files");
                var qry = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var result = new PagedUserSyncProcesseItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(filters, null, cancellationToken: cancellationToken),
                    UserSyncProcesseItems = await qry.ToListAsync(cancellationToken)
                };

                return Result.Ok(result);
            }

            private FilterDefinition<UserSyncProcesseItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<UserSyncProcesseItem>.Empty;
                var builder = Builders<UserSyncProcesseItem>.Filter;

                if (request.Filters == null) return filters;

                if (request.Filters.FromDate.HasValue && request.Filters.ToDate.HasValue)
                {
                    var fromDate = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(request.Filters.FromDate.Value));
                    var toDate = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(request.Filters.ToDate.Value));

                    filters = filters & builder.Gte("createdAt", fromDate);
                    filters = filters & builder.Lte("createdAt", toDate);
                }

                return filters;
            }
        }
    }
}
