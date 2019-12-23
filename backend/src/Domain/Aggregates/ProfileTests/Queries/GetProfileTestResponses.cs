using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestResponse;

namespace Domain.Aggregates.ProfileTests.Queries
{
    public class GetProfileTestResponses
    {
        public class Contract : CommandContract<Result<PagedItems>>
        {
            public string UserRole { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public string TestId { get; set; }
        }

        public class PagedItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<ResponseItem> Responses { get; set; }
        }

        public class ResponseItem
        {
            public ObjectId Id { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string UserName { get; set; }
            public string UserRegisterId { get; set; }
            public ObjectId TestId { get; set; }
            public string TestTitle { get; set; }
            public bool Recommended { get; set; }
            public List<ProfileTestAnswer> Answers { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail<PagedItems>("Acesso Negado");

                var options = new FindOptions<ResponseItem>() {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };
                
                var filters = SetFilters(request);
                var collection = _db.Database.GetCollection<ResponseItem>("ProfileTestResponses");

                var query = await collection.FindAsync(
                    filters, options, cancellationToken
                );

                var result = new PagedItems()
                {
                    Page = request.Page,
                    ItemsCount = await collection.CountDocumentsAsync(
                        filters, cancellationToken: cancellationToken
                    ),
                    Responses = await query.ToListAsync(cancellationToken)
                };

                return Result.Ok(result);
            }

            private FilterDefinition<ResponseItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<ResponseItem>.Empty;
                var builder = Builders<ResponseItem>.Filter;

                filters = filters & builder.Exists(x => x.Answers[0]);

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.TestId))
                {
                    filters = filters & builder.Eq(
                        x => x.TestId,
                        ObjectId.Parse(request.Filters.TestId)
                    );
                }

                return filters;
            }
        }
    }
}
