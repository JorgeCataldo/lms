using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.NetPromoterScores.Queries
{
    public class GetFilteredNpsQuery
    {
        public class Contract : CommandContract<Result<PagedNpsItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
        }

        public class RequestFilters
        {
            public string Name { get; set; }
            public string SortBy { get; set; }
            public bool IsSortAscending { get; set; }
        }

        public class PagedNpsItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<NpsItem> NpsItems { get; set; }
        }

        public class NpsItem
        {
            public ObjectId UserId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public decimal Grade { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
            public List<UserProgress> EventsInfo { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedNpsItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedNpsItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "Secretary")
                    return Result.Fail<PagedNpsItems> ("Acesso Negado");

                var options = new FindOptions<NpsItem>()
                {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<NpsItem> filters = SetFilters(request);

                if (!String.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.IsSortAscending)
                    {
                        options.Sort = Builders<NpsItem>.Sort.Ascending(request.Filters.SortBy);
                    }
                    else
                    {
                        options.Sort = Builders<NpsItem>.Sort.Descending(request.Filters.SortBy);
                    }
                }

                var collection = _db.Database.GetCollection<NpsItem>("NetPromoterScores");
                var qry = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var result = new PagedNpsItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(filters, null, cancellationToken: cancellationToken),
                    NpsItems = await qry.ToListAsync(cancellationToken)
                };

                return Result.Ok(result);
            }

            private IMongoQueryable<NpsItem> SetFilters(Contract request, IMongoQueryable<NpsItem> queryable)
            {
                if (request.Filters == null) return queryable;

                if (!String.IsNullOrEmpty(request.Filters.Name))
                {
                    queryable = queryable.Where(user =>
                        user.Name.ToLower()
                            .Contains(request.Filters.Name.ToLower())
                        );
                }

                return queryable;
            }
            private FilterDefinition<NpsItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<NpsItem>.Empty;
                var builder = Builders<NpsItem>.Filter;

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.Name))
                    filters = filters & builder.Regex("name",
                                  new BsonRegularExpression("/" + request.Filters.Name + "/is"));

                return filters;
            }

        }
    }
}
