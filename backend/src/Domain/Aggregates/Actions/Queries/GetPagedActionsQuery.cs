using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Actions.Queries
{
    public class GetPagedActionsQuery
    {
        public class Contract : CommandContract<Result<PagedActionItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public DateTimeOffset? InitialDate { get; set; }
            public DateTimeOffset? FinalDate { get; set; }
        }

        public class PagedActionItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<ActionItem> Actions { get; set; }
        }

        public class ActionItem
        {
            public ObjectId Id { get; set; }
            public int PageId { get; set; }
            public string Description { get; set; }
            public int TypeId { get; set; }
            public string ModuleId { get; set; }
            public string EventId { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
            public string Concept { get; set; }
            public string SupportMaterialId { get; set; }
            public string UserId { get; set; }
            public string QuestionId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public ObjectId CreatedBy { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedActionItems>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<PagedActionItems>> Handle(
                Contract request, CancellationToken cancellationToken
            ) {
                try
                {
                    var options = new FindOptions<ActionItem>()
                    {
                        Limit = request.PageSize,
                        Skip = (request.Page - 1) * request.PageSize
                    };
                    
                    FilterDefinition<ActionItem> filters = SetFilters(request);
                    var collection = _db.Database.GetCollection<ActionItem>("Actions");

                    var qry = await collection.FindAsync(filters,
                        options: options,
                        cancellationToken: cancellationToken
                    );

                    var actionsList = await qry.ToListAsync(cancellationToken);

                    var result = new PagedActionItems()
                    {
                        Page = request.Page,
                        ItemsCount = await collection.CountDocumentsAsync(
                            filters, null,
                            cancellationToken: cancellationToken
                        ),
                        Actions = actionsList
                    };


                    return Result.Ok(result);
                }
                catch (Exception err)
                {
                    return Result.Fail<PagedActionItems>(
                        $"Ocorreu um erro ao buscar as ações: {err.Message}"
                    );
                }

            }

            private FilterDefinition<ActionItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<ActionItem>.Empty;
                var builder = Builders<ActionItem>.Filter;

                if (request.InitialDate != null && request.FinalDate != null)
                {
                    filters = filters & builder.Gte(
                        x => x.CreatedAt,
                        request.InitialDate.Value
                    );

                    filters = filters & builder.Lte(
                        x => x.CreatedAt,
                        request.FinalDate.Value.AddHours(23).AddMinutes(59)
                    );

                }

                return filters;
            }
        }
    }
}
