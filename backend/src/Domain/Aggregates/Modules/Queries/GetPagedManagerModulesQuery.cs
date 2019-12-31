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
    public class GetPagedManagerModulesQuery
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
            // public ObjectId? InstructorId { get; set; }
            public string Title { get; set; }
            //public bool Published { get; set; }
            public string Excerpt { get;set; }
            //public string Instructor { get;set; }
            public string ImageUrl { get;set; }
            public string[] Tags { get; set; }
            //public List<SubjectItem> Subjects { get; set; }
            //public List<Requirement> Requirements { get; set; }
            public DateTimeOffset DeletedAt { get; set; }
            //public ObjectId DeletedBy { get; set; }
            //public List<ObjectId> TutorsIds { get; set; }
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
        public class SubordinateItem
        {
            public ObjectId UserId { get; set; }
            public List<User.UserProgress> UserModules { get; set; }
            public List<User.UserProgress> UserTracks { get; set; }
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

                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Fail<PagedModuleItems>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var subordinatesModulesIds = new List<ObjectId>();
                var subordinatesTracksIds = new List<ObjectId>();
                var modulesCollection = _db.Database.GetCollection<ModuleItem>("Modules");
                var modulesQuery = modulesCollection.AsQueryable();

                var responsible = await _db.ResponsibleCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId);

                if (responsible == null)
                    return Result.Fail<PagedModuleItems>("Acesso Negado");

                var subordinates = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => responsible.SubordinatesUsersIds.Contains(x.Id))
                    .Select(x => new SubordinateItem
                    {
                        UserId = x.Id,
                        UserModules = x.ModulesInfo,
                        UserTracks = x.TracksInfo
                    })
                    .ToListAsync();

                foreach(SubordinateItem sub in subordinates)
                {
                    if (sub.UserModules != null && sub.UserModules.Count > 0)
                        subordinatesModulesIds.AddRange(sub.UserModules.Select(x => x.Id));

                    if (sub.UserTracks != null && sub.UserTracks.Count > 0)
                        subordinatesTracksIds.AddRange(sub.UserTracks.Select(x => x.Id));
                }
                subordinatesTracksIds = subordinatesTracksIds.Distinct().ToList();

                var subordinatesTracksModulesConfiguration = await _db.TrackCollection
                    .AsQueryable()
                    .Where(x => subordinatesTracksIds.Contains(x.Id))
                    .Select(x => x.ModulesConfiguration)
                    .ToListAsync();

                foreach (List<Tracks.TrackModule> subTrackModule in subordinatesTracksModulesConfiguration)
                {
                    if (subTrackModule != null && subTrackModule.Count > 0)
                        subordinatesModulesIds.AddRange(subTrackModule.Select(x => x.ModuleId));
                }

                subordinatesModulesIds = subordinatesModulesIds.Distinct().ToList();

                modulesQuery = modulesQuery.Where(x =>
                    subordinatesModulesIds.Contains(x.Id) &&
                    x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                );

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

                var selectQuery = modulesQuery
                    .OrderBy(x =>
                        x.Title
                    )
                    .Skip(
                        (request.Page - 1) * request.PageSize
                    ).Take(
                        request.PageSize
                    );

                var modulesList = await selectQuery.ToListAsync(cancellationToken);

                var result = new PagedModuleItems()
                {
                    Page = request.Page,
                    ItemsCount = await modulesQuery.CountAsync(),
                    Modules = modulesList
                };

                return Result.Ok(result);
               
            }
        }
    }
}
