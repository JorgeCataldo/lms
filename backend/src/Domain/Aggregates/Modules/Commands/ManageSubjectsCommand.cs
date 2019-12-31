using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class ManageSubjectsCommand
    {
        public class Contract : CommandContract<Result<List<ContractSubject>>>
        {
            public string ModuleId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractSubject> Subjects { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractSubject
        {
            public string Id { get; set; }
            public string[] Concepts { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public List<ContentItem> Contents { get; set; }
            public List<ContractUserProgress> UserProgresses { get; set; }
        }

        public class ContentItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public long Duration { get; set; }
            public List<string> ReferenceUrls { get; set; }
            public ConceptItem[] Concepts { get; set; }
            public string Value { get; set; }
            public ContentType Type { get; set; }
        }

        public class ConceptItem
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
        }

        public class ContractUserProgress
        {
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ContractSubject>>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<List<ContractSubject>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<List<ContractSubject>>("Acesso Negado");

                var mId = ObjectId.Parse(request.ModuleId);
                var module = await (await _db
                    .Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                    return Result.Fail<List<ContractSubject>>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!Module.IsInstructor(module, userId).Data)
                        return Result.Fail<List<ContractSubject>>("Acesso Negado");
                }

                if (request.DeleteNonExistent)
                {
                    var existingIds = from o in request.Subjects
                        where !string.IsNullOrEmpty(o.Id)
                        select ObjectId.Parse(o.Id);
                    // deixando apenas os subjects que vieram na coleção
                    var include = from c in module.Subjects    
                        where existingIds.Contains(c.Id)    
                        select c;
                    module.Subjects = include.ToList();
                }
                //Criando os assuntos
                foreach (var csubject in request.Subjects)
                {
                    Subject subject = null;
                    if (string.IsNullOrEmpty(csubject.Id))
                    {
                        var subjectResult = Subject.Create(csubject.Title, csubject.Excerpt, csubject.Concepts);
                        if (subjectResult.IsFailure)
                        {
                            return Result.Fail<List<ContractSubject>>(subjectResult.Error);
                        }

                        subject = subjectResult.Data;
                        subject.UpdatedAt = DateTimeOffset.UtcNow;
                        module.Subjects.Add(subject);
                    }
                    else
                    {
                        subject = module.Subjects.FirstOrDefault(x => x.Id == ObjectId.Parse(csubject.Id));
                        if (subject == null)
                        {
                            return Result.Fail<List<ContractSubject>>($"Assunto não encontrado ({csubject.Id} - {csubject.Title})");
                        }

                        subject.Title = csubject.Title;
                        subject.Excerpt = csubject.Excerpt;

                        var clist = Concept.GetConcepts(csubject.Concepts);
                        if (clist.IsFailure) 
                            return Result.Fail<List<ContractSubject>>(clist.Error);

                        subject.Concepts = clist.Data;
                    }
                    
                    if (csubject.UserProgresses != null)
                    {
                        subject.UserProgresses = new List<UserProgress>();

                        foreach (var crequirements in csubject.UserProgresses)
                        {
                            var req = UserProgress.Create(ProgressType.SubjectProgress, crequirements.Level,
                                crequirements.Percentage);
                            if (req.IsFailure)
                            {
                                return Result.Fail<List<ContractSubject>>(req.Error);
                            }

                            subject.UserProgresses.Add(req.Data);
                        }
                    }
                }

                await _db.ModuleCollection.ReplaceOneAsync(t => t.Id == module.Id, module, cancellationToken: cancellationToken);

                var response = module.Subjects.Select(subject =>
                    new ContractSubject()
                    {
                        Concepts = subject.Concepts.Select(c => c.Name).ToArray(),
                        Excerpt = subject.Excerpt,
                        Id = subject.Id.ToString(),
                        Title = subject.Title,
                        Contents = subject.Contents.Select(c => new ContentItem()
                        {
                            Duration = c.Duration,
                            Excerpt = c.Excerpt,
                            Id = c.Id,
                            ReferenceUrls = c.ReferenceUrls,
                            Title = c.Title,
                            Type = c.Type,
                            Value = c.Value,
                            Concepts = c.Concepts.Select(concept => new ConceptItem()
                            {
                                Anchors = concept.Anchors,
                                Positions = concept.Positions,
                                Name = concept.Name
                            }).ToArray(),
                        }).ToList(),
                        UserProgresses = subject.UserProgresses.Select(up => new ContractUserProgress
                        {
                            Level = up.Level,
                            Percentage = up.Percentage
                        }).ToList()
                    }
                ).ToList();

                return Result.Ok( response );
            }
        }
    }
}
