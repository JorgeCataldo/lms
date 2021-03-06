﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.QuestionsDraft.Queries
{
    public class GetPagedQuestionsDraft
    {
        public class Contract : CommandContract<Result<PagedQuestionItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
        }

        public class RequestFilters
        {
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string Term { get; set; }
        }

        public class PagedQuestionItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<QuestionItem> Questions { get; set; }
        }


        public class QuestionItem
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId Id { get; set; }
            public string Text { get; set; }
            public List<AnswerItem> Answers { get; set; }
            public string[] Concepts { get; set; }
            public int Level { get; set; }
            public int Duration { get; set; }
        }

        public class AnswerItem
        {
            public string Description { get; set; }
            public int Points { get; set; }
            public List<AnswerConceptItem> Concepts { get; set; }
        }

        public class AnswerConceptItem
        {
            public string Concept { get; set; }
            public bool IsRight { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedQuestionItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedQuestionItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var options = new FindOptions<QuestionItem>()
                {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<QuestionItem> filters = SetFilters(request);
                var modulesCollection = _db.Database.GetCollection<QuestionItem>("QuestionsDrafts");

                var qry = await modulesCollection.FindAsync(filters,
                    options: options,
                    cancellationToken: cancellationToken
                );

                var result = new PagedQuestionItems()
                {
                    Page = request.Page,
                    ItemsCount = await modulesCollection.CountDocumentsAsync(
                        filters, null, cancellationToken: cancellationToken
                    ),
                    Questions = await qry.ToListAsync(cancellationToken)
                };

                return Result.Ok(result);

            }

            private FilterDefinition<QuestionItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<QuestionItem>.Empty;
                var builder = Builders<QuestionItem>.Filter;

                filters = filters & (
                    builder.Eq("deletedAt", DateTimeOffset.MinValue) |
                    builder.Eq("deletedAt", BsonNull.Value)
                );

                filters = filters & builder.Eq("draftPublished", false);

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.SubjectId))
                    filters = filters & builder.Eq("subjectId", ObjectId.Parse(request.Filters.SubjectId));

                if (!String.IsNullOrEmpty(request.Filters.ModuleId))
                {
                    var modId = ObjectId.Parse(request.Filters.ModuleId);
                    filters = filters & (
                        builder.Eq("moduleId", modId) |
                        builder.Eq("draftId", modId)   
                    );
                }

                if (!String.IsNullOrEmpty(request.Filters.Term))
                    filters = filters & builder.Regex("text",
                                  new BsonRegularExpression("/" + request.Filters.Term + "/is"));

                return filters;
            }
        }
    }
}

