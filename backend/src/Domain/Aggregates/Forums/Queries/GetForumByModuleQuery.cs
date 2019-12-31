using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Data;
using Domain.Base;
using Domain.Extensions;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Queries
{
    public class GetForumByModuleQuery
    {
        public class Contract : CommandContract<Result<PagedForumItem>>
        {
            public string ModuleId { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserId { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
        }

        public class ForumItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ModuleId { get; set; }
            public string ModuleName { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class PagedForumItem
        {
            public string ModuleId { get; set; }
            public string ModuleName { get; set; }
            public long ItemsCount { get; set; }
            public bool IsInstructor { get; set; }
            public List<ForumQuestionItem> Questions { get; set; }
            public List<ForumQuestionItem> LastQuestions { get; set; }
            public List<SubjectItem> Subjects { get; set; }
        }

        public class ForumQuestionItem
        {
            public ObjectId Id { get; set; }
            public ObjectId? SubjectId { get; set; }
            public string SubjectName { get; set; }
            public ObjectId? ContentId { get; set; }
            public string ContentName { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool Liked { get; set; }
            public List<string> LikedBy { get; set; }
            public List<ForumAnswer> Answers { get; set; } = new List<ForumAnswer>();
            public DateTimeOffset CreatedAt { get; set; }
        }

        public class SubjectItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ContentItem> Contents { get; set; }
        }

        public class ContentItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
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
                    if (String.IsNullOrEmpty(request.ModuleId))
                        return Result.Fail<PagedForumItem>("Id do Módulo não informado");

                    var forum = await GetForum(request.ModuleId, cancellationToken);

                    if (forum == null)
                    {
                        var newForum = await CreateForum(request.ModuleId);

                        if (newForum.IsFailure)
                            return Result.Fail<PagedForumItem>(newForum.Error);
                        
                        return Result.Ok(
                            new PagedForumItem() {
                                ModuleId = newForum.Data.ModuleId.ToString(),
                                ModuleName = newForum.Data.ModuleName,
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

            private async Task<ForumItem> GetForum(string rModuleId, CancellationToken cancellationToken)
            {
                var moduleId = ObjectId.Parse(rModuleId);
                var query = await _db.Database
                    .GetCollection<ForumItem>("Forums")
                    .FindAsync(x => x.ModuleId == moduleId);

                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            }

            private async Task<Result<Forum>> CreateForum(string moduleId)
            {
                var mdId = ObjectId.Parse(moduleId);

                var qry = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == mdId);

                var module = await qry.FirstOrDefaultAsync();

                var newForum = Forum.Create(
                    module.Id,
                    module.Title,
                    new List<ObjectId>()
                );

                await _db.ForumCollection.InsertOneAsync(newForum.Data);

                return newForum;
            }

            private async Task<PagedForumItem> GetQuestions(ForumItem forum, Contract request, CancellationToken cancellationToken)
            {
                var options = new FindOptions<ForumQuestionItem>() {
                    Limit = request.PageSize,
                    Skip = (request.Page - 1) * request.PageSize
                };

                FilterDefinition<ForumQuestionItem> filters = SetFilters(request);
                var collection = _db.Database.GetCollection<ForumQuestionItem>("ForumQuestions");

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
                    .GetCollection<ForumAnswer>("ForumAnswers")
                    .FindAsync(x => x.QuestionId == question.Id))
                    .ToListAsync();
                }
                
                var lastQuestions = await queryLast.ToListAsync(cancellationToken);
                lastQuestions = lastQuestions.OrderByDescending(x => x.CreatedAt).Take(4).ToList();

                return new PagedForumItem() {
                    ModuleId = forum.ModuleId.ToString(),
                    ModuleName = forum.ModuleName,
                    IsInstructor = await CheckModuleInstructor(
                        ObjectId.Parse(request.ModuleId),
                        ObjectId.Parse(request.UserId),
                        cancellationToken
                    ),
                    ItemsCount = await collection.CountDocumentsAsync(filters),
                    Questions = questions,
                    LastQuestions = lastQuestions,
                    Subjects = GetSubjects(questions)
                };
            }

            private FilterDefinition<ForumQuestionItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<ForumQuestionItem>.Empty;
                var builder = Builders<ForumQuestionItem>.Filter;

                filters = filters & builder.Eq("moduleId", ObjectId.Parse(request.ModuleId));

                if (request.Filters == null) return filters;

                if (!String.IsNullOrEmpty(request.Filters.SubjectId))
                    filters = filters & builder.Eq("subjectId", ObjectId.Parse(request.Filters.SubjectId));

                if (!String.IsNullOrEmpty(request.Filters.ContentId))
                    filters = filters & builder.Eq("contentId", ObjectId.Parse(request.Filters.ContentId));

                if (!String.IsNullOrEmpty(request.Filters.Term))
                {
                    filters = filters &
                        (GetRegexFilter(builder, "title", request.Filters.Term) |
                         GetRegexFilter(builder, "description", request.Filters.Term));
                }
                    
                return filters;
            }

            private List<SubjectItem> GetSubjects(List<ForumQuestionItem> questions)
            {
                var subjects = questions
                    .Where(q => q.SubjectId.HasValue)
                    .Select(q => new SubjectItem() {
                         Id = q.SubjectId.Value,
                         Title = q.SubjectName
                     })
                    .DistinctBy(x => x.Id)
                    .ToList();

                foreach (var subject in subjects)
                {
                    subject.Contents = questions
                        .Where(q =>
                            q.ContentId.HasValue &&
                            q.SubjectId.HasValue &&
                            q.SubjectId == subject.Id)
                        .Select(q => new ContentItem() {
                            Id = q.ContentId.Value,
                            Title = q.ContentName
                        })
                        .DistinctBy(x => x.Id)
                        .ToList();
                }

                return subjects;
            }

            private FilterDefinition<ForumQuestionItem> GetRegexFilter(
                FilterDefinitionBuilder<ForumQuestionItem> builder, string prop, string term
            ) {
                return builder.Regex(prop,
                    new BsonRegularExpression("/" + term + "/is")
                );
            }

            private async Task<bool> CheckModuleInstructor(
                ObjectId moduleId, ObjectId userId, CancellationToken cancellationToken
            ) {
                var query = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x =>
                        x.Id == moduleId &&
                        (
                            (x.InstructorId.HasValue && x.InstructorId.Value == userId) ||
                            (x.ExtraInstructorIds.Any() && x.ExtraInstructorIds.Contains(userId))
                        ),
                        cancellationToken: cancellationToken
                    );
                return await query.AnyAsync();
            }
        }
    }
}
