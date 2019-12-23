using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Users;
using Domain.Aggregates.UserProgressHistory;
using Action = Domain.Aggregates.Modules.Action;
using Domain.Aggregates.UserFiles;
using Domain.ValueObjects;
using Domain.Enumerations;
using Domain.Base;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetAllContentQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string TrackId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ModuleItem> Modules { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public List<Subject> Subjects { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<SubjectItem> Subjects { get; set; }
        }
        public class SubjectItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<ContentItem> Contents { get; set; }
            public List<Concept> Concepts { get; set; }
        }

        public class ContentItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public long Duration { get; set; }
            public string Value { get; set; }
            public ContentType Type { get; set; }
            public string Excerpt { get; set; }
            public List<string> ReferenceUrls { get; set; }
            public ConceptItem[] Concepts { get; set; }
            public int? NumPages { get; set; }
            public bool Watched { get; set; }
        }

        public class ConceptItem
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<TrackItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<TrackItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
               
                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<TrackItem>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);

                var track = await GetTrackById(trackId, cancellationToken);

                var trackItem = new TrackItem
                {
                    Id = track.Id,
                    Title = track.Title,
                    Modules = new List<ModuleItem>()
                };

                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                var typeResult = ActionType.Create(3);

                var actionsColl = _db.Database.GetCollection<Action>("Actions");
                var actionsQuery = await actionsColl.FindAsync(x =>
                    x.Type == typeResult.Data &&
                    x.CreatedBy == ObjectId.Parse(request.UserId)
                );
                var actions = await actionsQuery.ToListAsync();

                var watchedContents = actions.Select(x => x.ContentId).ToList();
                watchedContents = watchedContents.Distinct().ToList();

                var moduleIds = track.ModulesConfiguration.Select(x => x.ModuleId).ToList();

                var qry = await _db.Database
                        .GetCollection<ModuleItem>("Modules")
                        .FindAsync(x => moduleIds.Contains(x.Id), cancellationToken: cancellationToken);

                var modules = await qry.ToListAsync(cancellationToken: cancellationToken);

                if (modules != null)
                {
                    for (int i = 0; i < modules.Count; i++)
                    {
                        for (int j = 0; j < modules[i].Subjects.Count; j++)
                        {
                            for (int k = 0; k < modules[i].Subjects[j].Contents.Count; k++)
                            {
                                var contentId = modules[i].Subjects[j].Contents[k].Id.ToString();
                                bool watched = watchedContents.Contains(contentId);
                                modules[i].Subjects[j].Contents[k].Watched = watched;
                            }
                        }
                    }
                    trackItem.Modules = modules;
                }   

                return Result.Ok(trackItem);
            }


            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

        }
    }
}
