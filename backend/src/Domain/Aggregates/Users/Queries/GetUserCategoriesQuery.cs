using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserCategoriesQuery
    {
        public class Contract : CommandContract<Result<ListCategoryItems>>
        {
            public string SearchTerm { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int Category { get; set; }
        }

        public class ListCategoryItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<CategoryItem> Items { get; set; }
        }

        public enum CategoryEnum
        {
            Jobs = 1,
            Ranks = 2,
            Segments = 3,
            Sectors = 4,
            BusinessGroups = 5,
            BusinessUnits = 6,
            FrontBackOffices = 7,
            Users = 8,
            Countries = 9,
            Locations = 10,
            Tracks = 11,
            Modules = 12,
            Badges = 13,
            PastEvents = 14
        }

        public class CategoryItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public ObjectId? CountryId { get; set; }
            public DateTimeOffset? DeletedAt { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ListCategoryItems>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<ListCategoryItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var filter = FilterDefinition<CategoryItem>.Empty;
                var builder = Builders<CategoryItem>.Filter;

                bool isAlternate = request.Category > 10;

                filter = filter & (
                    builder.Eq(x => x.DeletedAt, null) |
                    builder.Eq(x => x.DeletedAt, DateTimeOffset.MinValue)
                );

                if (!String.IsNullOrEmpty(request.SearchTerm))
                {
                    filter = isAlternate ?
                        filter & builder.Regex("title",
                            new BsonRegularExpression("/" + request.SearchTerm + "/is")
                        ) :
                        filter & builder.Regex("name",
                            new BsonRegularExpression("/" + request.SearchTerm + "/is")
                        );
                }

                string sortingProp = isAlternate ? "Title" : "Name";

                var options = new FindOptions<CategoryItem>() {
                    Sort = Builders<CategoryItem>.Sort.Ascending(sortingProp),
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                var collection = GetCollectionByCategory(
                    (CategoryEnum) request.Category
                );

                if (collection == null)
                    return Result.Fail<ListCategoryItems>("Categoria não existe");
                    
                var query = await collection.FindAsync(
                    filter, options,
                    cancellationToken: cancellationToken
                );
                    
                return Result.Ok(
                    new ListCategoryItems() {
                        Page = request.Page,
                        ItemsCount = await collection.CountDocumentsAsync(
                            filter, null,
                            cancellationToken: cancellationToken
                        ),
                        Items = await query.ToListAsync(cancellationToken)
                    }
                );
            }

            private IMongoCollection<CategoryItem> GetCollectionByCategory(CategoryEnum category)
            {
                switch (category)
                {
                    case CategoryEnum.Jobs:
                        return _db.Database.GetCollection<CategoryItem>("Jobs");
                    case CategoryEnum.Ranks:
                        return _db.Database.GetCollection<CategoryItem>("Ranks");
                    case CategoryEnum.Segments:
                        return _db.Database.GetCollection<CategoryItem>("Segments");
                    case CategoryEnum.Sectors:
                        return _db.Database.GetCollection<CategoryItem>("Sectors");
                    case CategoryEnum.BusinessGroups:
                        return _db.Database.GetCollection<CategoryItem>("BusinessGroups");
                    case CategoryEnum.BusinessUnits:
                        return _db.Database.GetCollection<CategoryItem>("BusinessUnits");
                    case CategoryEnum.FrontBackOffices:
                        return _db.Database.GetCollection<CategoryItem>("FrontBackOffices");
                    case CategoryEnum.Users:
                        return _db.Database.GetCollection<CategoryItem>("Users");
                    case CategoryEnum.Countries:
                        return _db.Database.GetCollection<CategoryItem>("Countries");
                    case CategoryEnum.Locations:
                        return _db.Database.GetCollection<CategoryItem>("Locations");
                    case CategoryEnum.Tracks:
                        return _db.Database.GetCollection<CategoryItem>("Tracks");
                    case CategoryEnum.Modules:
                        return _db.Database.GetCollection<CategoryItem>("Modules");
                    default:
                        return null;
                }
            }
        }
    }
}
