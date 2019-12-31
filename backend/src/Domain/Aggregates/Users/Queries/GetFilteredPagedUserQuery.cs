using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.UserProgressHistory;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Queries
{
    public class GetFilteredPagedUserQuery
    {
        public class Contract : CommandContract<Result<PagedUserItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
            public ObjectId UserId { get; set; }
            public bool SelectAllUsers { get; set; }
        }

        public class RequestFilters
        {
            public string Term { get; set; }
            public List<UserCategoryFilter> CategoryFilter { get; set; }
            public string Dependent { get; set; }
            public DateTimeOffset? CreatedSince { get; set; }
            public string SortBy { get; set; }
            public bool? IsSortAscending { get; set; }
        }

        public class UserCategoryFilter
        {
            public string ColumnName { get; set; }
            public List<string> ContentNames { get; set; }
        }

        public class PagedUserItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<UserItem> UserItems { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string LineManager { get; set; }
            public string UserName { get; set; }
            public string RegistrationId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public User.RelationalItem Rank { get; set; }
            public List<User.UserProgress> TracksInfo { get; set; }
            public List<User.UserProgress> ModulesInfo { get; set; }
            public DateTimeOffset? BirthDate { get; set; }
            public User.RelationalItem Location { get; set; }
            public bool IsBlocked { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedUserItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedUserItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var collection = _db.Database.GetCollection<User>("Users");
                var queryable = collection.AsQueryable();

                queryable = SetFilters(request, queryable);

                var users = await queryable
                    .Select(u => new UserItem {
                        Id = u.Id,
                        CreatedAt = u.CreatedAt,
                        ImageUrl = u.ImageUrl,
                        LineManager = u.LineManager,
                        Name = u.Name,
                        Email = u.Email,
                        Rank = u.Rank,
                        ResponsibleId = u.ResponsibleId,
					    UserName = u.UserName,
					    RegistrationId = u.RegistrationId,
                        TracksInfo = u.TracksInfo,
                        BirthDate = u.BirthDate,
                        Location = u.Location,
                        IsBlocked = u.IsBlocked
                    }).ToListAsync();

                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    var usersIds = users.Select(u => u.Id).ToList();
                    var progressList = new List<UserModuleProgress>();
                    var applications = new List<EventApplication>();

                    var checkProgress = request.Filters.CategoryFilter.Any(f =>
                        (f.ColumnName == "module.id" || f.ColumnName == "level.id") &&
                        f.ContentNames.Count > 0
                    );

                    var checkApplications = request.Filters.CategoryFilter.Any(f =>
                        f.ColumnName == "event.id" &&
                        f.ContentNames.Count > 0
                    );

                    if (checkProgress)
                        progressList = await GetModuleProgress(usersIds);

                    if (checkApplications)
                        applications = await GetApplications(usersIds);

                    foreach (UserCategoryFilter userCategoryFilter in request.Filters.CategoryFilter)
                    {
                        foreach (string contentName in userCategoryFilter.ContentNames)
                        {
                            users = FilterByProgress(
                                users, userCategoryFilter.ColumnName, contentName,
                                progressList, applications
                            );
                        }
                    }
                }

                var result = new PagedUserItems();

                if (!request.SelectAllUsers)
                {
                    result = new PagedUserItems()
                    {
                        Page = request.Page,
                        ItemsCount = users.Count,
                        UserItems = users
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList()
                    };
                }
                else
                {
                    result = new PagedUserItems()
                    {
                        Page = request.Page,
                        ItemsCount = users.Count,
                        UserItems = users.ToList()
                    };
                }

                return Result.Ok(result);
            }

            private IMongoQueryable<User> SetFilters(Contract request, IMongoQueryable<User> queryable)
            {
                queryable = queryable.Where(u =>
                    u.Id != request.UserId
                );

                if (request.UserRole == "Student")
                    queryable = queryable.Where(u => u.ResponsibleId == request.UserId);

                if (request.Filters == null) return queryable;

                if (!String.IsNullOrEmpty(request.Filters.Term))
                {
                    queryable = queryable.Where(user =>
                        user.Name.ToLower()
                            .Contains(request.Filters.Term.ToLower()) ||
                        user.Email.ToLower()
                            .Contains(request.Filters.Term.ToLower())                        
                        );
                }

                if (request.Filters.CreatedSince.HasValue)
                {
                    queryable = queryable.Where(user =>
                        user.CreatedAt >= request.Filters.CreatedSince.Value.Date
                    );
                }

                if (request.UserRole == "Student" && !String.IsNullOrEmpty(request.Filters.Dependent))
                {
                    queryable = queryable.Where(user =>
                        user.LineManager.ToLower()
                            .Contains(request.Filters.Dependent.ToLower())
                    );
                }

                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    foreach (UserCategoryFilter userCategoryFilter in request.Filters.CategoryFilter)
                    {
                        foreach (string contentName in userCategoryFilter.ContentNames)
                        {
                            queryable = FilterByCategory(
                                queryable, userCategoryFilter.ColumnName, contentName
                            );
                        }
                    }
                }

                if (!string.IsNullOrEmpty(request.Filters.SortBy))
                {
                    if (request.Filters.SortBy == "name")
                    {
                        if (request.Filters.IsSortAscending.Value)
                            queryable = queryable.OrderBy(user => user.Name);
                        else
                            queryable = queryable.OrderByDescending(user => user.Name);
                    }
                }

                return queryable;
            }

            private IMongoQueryable<User> FilterByCategory(
                IMongoQueryable<User> queryable, string columnName, string value
            ) {
                switch (columnName)
                {
                    case "businessGroup.name":
                        return queryable.Where(u => u.BusinessGroup.Name == value);
                    case "businessUnit.name":
                        return queryable.Where(u => u.BusinessUnit.Name == value);
                    case "job.name":
                        return queryable.Where(u => u.Job.Name == value);
                    case "rank.name":
                        return queryable.Where(u => u.Rank.Name == value);
                    case "segment.name":
                        return queryable.Where(u => u.Segment.Name == value);
                    case "frontBackOffice.name":
                        return queryable.Where(u => u.FrontBackOffice.Name == value);
                    case "userType":
                        return FilterByUserType(queryable, value);
                    default:
                        return queryable;
                }
            }

            private IMongoQueryable<User> FilterByUserType(
                IMongoQueryable<User> queryable, string type
            )
            {
                switch (type)
                {
                    case "student":
                        return queryable.Where(u => u is Student);
                    case "secretary":
                        return queryable.Where(u => u is Secretary);
                    case "humanResources":
                        return queryable.Where(u => u is HumanResources);
                    case "recruiter":
                        return queryable.Where(u => u is Recruiter);
                    case "author":
                        return queryable.Where(u => u is Author);
                    case "admin":
                        return queryable.Where(u => u is Admin);
                    case "blocked":
                        return queryable.Where(u => u.IsBlocked == true);
                    case "notBlocked":
                        return queryable.Where(u => u.IsBlocked != true);
                    default:
                        return queryable;
                }
            }

            private List<UserItem> FilterByProgress(
                List<UserItem> users, string columnName, string value,
                List<UserModuleProgress> progressList, List<EventApplication> applications
            ) {
                switch (columnName)
                {
                    case "track.id":
                        return users.Where(u =>
                            u.TracksInfo != null &&
                            u.TracksInfo.Any(t => t.Id == ObjectId.Parse(value))
                        ).ToList();
                    case "module.id":
                        return users.Where(u =>
                            progressList.Find(p => 
                                p.UserId == u.Id &&
                                p.ModuleId == ObjectId.Parse(value)
                            ) != null
                        ).ToList();
                    case "level.id":
                        return users.Where(u =>
                            progressList.Any(p =>
                                p.Level > Int32.Parse(value)
                            )
                        ).ToList();
                    case "event.id":
                        return users.Where(u =>
                            applications.Any(app =>
                                app.UserId == u.Id &&
                                app.EventId == ObjectId.Parse(value)
                            )
                        ).ToList();
                    default:
                        return users;
                }
            }

            private async Task<List<UserModuleProgress>> GetModuleProgress(List<ObjectId> usersIds) {
                var collection = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                var queryable = collection.AsQueryable();

                return await queryable.Where(modProgress =>
                    usersIds.Contains(modProgress.UserId)
                ).ToListAsync();
            }

            private async Task<List<EventApplication>> GetApplications(List<ObjectId> usersIds)
            {
                var collection = _db.Database.GetCollection<EventApplication>("EventApplications");
                var queryable = collection.AsQueryable();

                return await queryable.Where(app =>
                    usersIds.Contains(app.UserId) &&
                    app.ApplicationStatus == ApplicationStatus.Approved &&
                    app.UserPresence.HasValue &&
                    app.UserPresence.Value
                ).ToListAsync();
            }
        }
    }
}
