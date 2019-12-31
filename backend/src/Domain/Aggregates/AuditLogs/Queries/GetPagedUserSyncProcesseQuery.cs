using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.AuditLogs.Queries
{
    public class GetPagedUserSyncProcesseQuery
    {
        public class Contract : IRequest<Result<PagedAuditLogItems>>
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
        
        public class PagedAuditLogItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<AuditLog> AuditLogItems { get; set; }
            public List<string> EntityTypes { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedAuditLogItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedAuditLogItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Admin" && request.CurrentUserRole != "HumanResources" && request.CurrentUserRole != "Recruiter")
                    return Result.Fail<PagedAuditLogItems>("Acesso Negado");

                var options = new FindOptions<AuditLog>()
                {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<AuditLog> filters = SetFilters(request);

                if (!string.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.IsSortAscending)
                    {
                        options.Sort = Builders<AuditLog>.Sort.Ascending(request.Filters.SortBy);
                    }
                    else
                    {
                        options.Sort = Builders<AuditLog>.Sort.Descending(request.Filters.SortBy);
                    }
                }

                var collection = _db.Database.GetCollection<AuditLog>("AuditLogs");
                var qry = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var result = new PagedAuditLogItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(filters, null, cancellationToken: cancellationToken),
                    AuditLogItems = await qry.ToListAsync(cancellationToken)
                };

                for (int i = 0; i < result.AuditLogItems.Count; i++)
                {
                    result.AuditLogItems[i].ActionDescription = Enum.GetName(typeof(EntityAction), (int)result.AuditLogItems[i].Action);
                }
                result.EntityTypes = result.AuditLogItems.Select(x => x.EntityType.ToString()).Distinct().ToList();

                return Result.Ok(result);
            }

            private FilterDefinition<AuditLog> SetFilters(Contract request)
            {
                var filters = FilterDefinition<AuditLog>.Empty;
                var builder = Builders<AuditLog>.Filter;

                if (request.Filters == null) return filters;

                if (request.Filters.FromDate.HasValue && request.Filters.ToDate.HasValue)
                {
                    var fromDate = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(request.Filters.FromDate.Value));
                    var toDate = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(request.Filters.ToDate.Value));

                    filters = filters & builder.Gte("date", fromDate);
                    filters = filters & builder.Lte("date", toDate);
                }

                return filters;
            }
        }
    }
}
