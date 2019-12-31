using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using Domain.Extensions;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Queries
{
    public class GetPagedModulesQuery
    {
        public class Contract : CommandContract<Result<PagedModuleItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class RequestFilters
        {
            public string[] Tags{ get; set; }
            public string Term { get; set; }
        }
        
        public class PagedModuleItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<ModuleItem> Modules { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId Id { get; set; }
            public ObjectId? InstructorId { get; set; }
            public string Title { get; set; }
            public bool Published { get; set; }
            public string Excerpt { get;set; }
            public string Instructor { get;set; }
            public string ImageUrl { get;set; }
            public string[] Tags { get; set; }
            public List<SubjectItem> Subjects { get; set; }
            public List<Requirement> Requirements { get; set; }
            public bool HasUserProgess { get; set; }
            public DateTimeOffset DeletedAt { get; set; }
            public ObjectId DeletedBy { get; set; }
            public List<ObjectId> TutorsIds { get; set; }
            public List<ObjectId> ExtraInstructorIds { get; set; }
            public List<EcommerceModule> EcommerceProducts { get; set; }
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
        }
        public class UserModuleItem
        {
            public ObjectId ModuleId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedModuleItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedModuleItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var modulesCollection = _db.Database.GetCollection<ModuleItem>("Modules");
                var modulesQuery = modulesCollection.AsQueryable();

                var usersModuleCollection = _db.Database.GetCollection<UserModuleItem>("UserModuleProgress");
                var usersModuleQuery = usersModuleCollection.AsQueryable();

                modulesQuery = modulesQuery.Where(x =>
                    x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                );

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    
                    modulesQuery = modulesQuery.Where(x =>
                        x.InstructorId == userId ||
                        x.ExtraInstructorIds.Contains(userId) ||
                        x.TutorsIds.Contains(userId)
                    );
                }

                if ( request.UserRole == "BusinessManager")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var user = await GetUserById(userId, cancellationToken);

                    var RecommendedModules = new List<ObjectId>();
                    if (user.ModulesInfo != null)
                        RecommendedModules = user.ModulesInfo.Select(m => m.Id).ToList();

                    modulesQuery = modulesQuery.Where(x =>
                        RecommendedModules.Contains(x.Id)
                    );
                }

                if (request.Filters != null)
                {
                    if (!String.IsNullOrEmpty(request.Filters.Term))
                    {
                        request.Filters.Term = request.Filters.Term.ToLower().RemoveDiacritics();
                        modulesQuery = modulesQuery.Where(x =>
                            x.Title.ToLower().Contains(request.Filters.Term) ||
                            x.Excerpt.ToLower().Contains(request.Filters.Term)
                        );
                    }

                    if (request.Filters.Tags != null && request.Filters.Tags.Length > 0)
                    {
                        modulesQuery = modulesQuery.Where(x =>
                            request.Filters.Tags.Any(
                                t => x.Tags.Contains(t)
                            )
                        );
                    }
                }
                
                var selectQuery = request.PageSize > 0 ? modulesQuery
                    .OrderBy(x =>
                        x.Title
                    )
                    .Skip(
                        (request.Page - 1) * request.PageSize
                    ).Take(
                        request.PageSize
                    ) : modulesQuery
                        .OrderBy(x =>
                            x.Title
                        );

                var modulesList = await selectQuery.ToListAsync(cancellationToken);

                foreach(ModuleItem module in modulesList)
                {
                    module.HasUserProgess = usersModuleQuery.FirstOrDefault(
                        x => x.ModuleId == module.Id
                    ) != null;
                }

                var result = new PagedModuleItems()
                {
                    Page = request.Page,
                    ItemsCount = await modulesQuery.CountAsync(),
                    Modules = modulesList
                };

                return Result.Ok(result);
               
            }

            private async Task<User> GetUserById(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(
                        x => x.Id == userId,
                        cancellationToken: token
                     );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }
        }
    }
}
