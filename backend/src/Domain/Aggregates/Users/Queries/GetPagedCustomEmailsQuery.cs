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
    public class GetPagedCustomEmailsQuery
    {
        public class Contract : CommandContract<Result<PagedCustomEmailItems>>
        {
            public string CurrentUserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public string SortBy { get; set; }
            public bool IsSortAscending { get; set; }
        }
        
        public class PagedCustomEmailItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<CustomEmailItem> CustomEmailItems { get; set; }
        }

        public class CustomEmailItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public List<ObjectId> UsersIds { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public List<UserInfoItem> Users { get; set; } = new List<UserInfoItem>();
        }

        public class UserInfoItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedCustomEmailItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedCustomEmailItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Admin" && request.CurrentUserRole == "Secretary" && request.CurrentUserRole == "HumanResources")
                    return Result.Fail<PagedCustomEmailItems>("Acesso Negado");

                var options = new FindOptions<CustomEmailItem>()
                {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<CustomEmailItem> filters = FilterDefinition<CustomEmailItem>.Empty;

                if (!String.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.IsSortAscending)
                    {
                        options.Sort = Builders<CustomEmailItem>.Sort.Ascending(request.Filters.SortBy);
                    }
                    else
                    {
                        options.Sort = Builders<CustomEmailItem>.Sort.Descending(request.Filters.SortBy);
                    }
                }

                var collection = _db.Database.GetCollection<CustomEmailItem>("CustomEmails");
                var qry = await collection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );
                var collectionList = await qry.ToListAsync(cancellationToken);

                foreach (CustomEmailItem email in collectionList)
                {
                    email.Users = await _db.UserCollection.AsQueryable()
                    .Where(x => email.UsersIds.Contains(x.Id))
                    .Select(x => new UserInfoItem { Id = x.Id, Name = x.Name, ImageUrl = x.ImageUrl })
                    .ToListAsync();
                }

                var result = new PagedCustomEmailItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(filters, null, cancellationToken: cancellationToken),
                    CustomEmailItems = collectionList
                };

                return Result.Ok(result);
            }
        }
    }
}
