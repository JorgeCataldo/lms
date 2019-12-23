using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.Users;
using Domain.Aggregates.ValuationTests;
using Domain.Base;
using Domain.Data;
using Domain.Enumerations;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Modules.Queries
{
    public class GetAllContentQuery
    {
        public class Contract : CommandContract<Result<ModuleItem>>
        {
            public string ModuleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
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

        public class Handler : IRequestHandler<Contract, Result<ModuleItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<ModuleItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {

                    //if (request.UserRole == "Admin" || request.UserRole == "HumanResources" || request.UserRole == "Student")
                    //{
                    if (String.IsNullOrEmpty(request.ModuleId))
                        return Result.Fail<ModuleItem>("Id da Trilha não informado");
                                       

                    var qry = await _db.Database
                            .GetCollection<ModuleItem>("Modules")
                            .FindAsync(x => x.Id == ObjectId.Parse(request.ModuleId), cancellationToken: cancellationToken);

                    var module = await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (module != null)
                    {
                        var typeResult = ActionType.Create(3);

                        var actionsColl = _db.Database.GetCollection<Action>("Actions");
                        var actionsQuery = await actionsColl.FindAsync(x =>
                            x.Type == typeResult.Data &&
                            x.ModuleId == request.ModuleId &&
                            x.CreatedBy == ObjectId.Parse(request.UserId)
                        );
                        var actions = await actionsQuery.ToListAsync();

                        var watchedContents = actions.Select(x => x.ContentId).ToList();
                        watchedContents = watchedContents.Distinct().ToList();

                        for (int j = 0; j < module.Subjects.Count; j++)
                        {
                            for (int k = 0; k < module.Subjects[j].Contents.Count; k++)
                            {
                                var contentId = module.Subjects[j].Contents[k].Id.ToString();
                                bool watched = watchedContents.Contains(contentId);
                                module.Subjects[j].Contents[k].Watched = watched;
                            }
                        }                        
                    }

                    return Result.Ok(module);
                }
                catch (Exception ex)
                {
                    return Result.Fail<ModuleItem>("Acesso Negado");
                }
            }            
        }
    }
}
