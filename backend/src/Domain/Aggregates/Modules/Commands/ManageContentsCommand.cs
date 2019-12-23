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
    public class ManageContentsCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string ModuleId { get; set; }
            public bool DeleteNonExistent { get; set; }
            public List<ContractContent> Contents { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractContent
        {
            /*
            public type: ContentTypeEnum;
            public title: string;
            public excerpt: string;
            public duration: number;
            public referenceUrls: Array<string>;
            public subjectId: string;
            public concepts: Array<ConceptReference | VideoReference | PdfReference | TextReference>;
            public value: string;
            */
            public string Id { get; set; }
            public string SubjectId { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public long Duration { get; set; }
            public ContentType Type { get; set; }
            public string[] ReferenceUrls { get; set; }
            public List<ContractConcepts> Concepts { get; set; }
            public string Value { get; set; }
            public int? numPages { get; set; }
        }

        public class ContractConcepts
        {
            public string Name { get; set; }
            public List<long> Positions { get; set; }
            public List<string> Anchors { get; set; }
        }

        public class Handler : IRequestHandler<ManageContentsCommand.Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                var module = await GetModule(request, cancellationToken);
                if (module == null)
                    return Result.Fail<bool>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!Module.IsInstructor(module, userId).Data)
                        return Result.Fail<bool>("Acesso Negado");
                }

                if (request.DeleteNonExistent)
                    module = DeleteNonExisting(module, request.Contents);

                foreach (var ccontent in request.Contents)
                {
                    var subjectId = ObjectId.Parse(ccontent.SubjectId);
                    var subject = module.Subjects.FirstOrDefault(x => x.Id == subjectId);
                    if (subject == null)
                        return Result.Fail<bool>("Assunto não Encontrado");

                    Content content = null;
                    if (string.IsNullOrEmpty(ccontent.Id))
                    {
                        var contentConcepts = ccontent.Concepts.Select(
                            x => new ConceptPosition(x.Name, x.Positions, x.Anchors)
                        ).ToList();
                        
                        var contentResult = Content.Create(
                            ccontent.Title, ccontent.Excerpt, contentConcepts, 
                            ccontent.ReferenceUrls, ccontent.Value,
                            ccontent.Type, ccontent.Duration, ccontent.numPages
                        );
                  
                        if (contentResult.IsFailure)
                            return Result.Fail<bool>(contentResult.Error);

                        content = contentResult.Data;
                        content.UpdatedAt = DateTimeOffset.UtcNow;
                        content.ReferenceUrls = new List<string>(ccontent.ReferenceUrls);

                        subject.Contents.Add(content);
                    }
                    else
                    {
                        content = subject.Contents.FirstOrDefault(x => x.Id == ObjectId.Parse(ccontent.Id));
                        if (content == null)
                            return Result.Fail<bool>($"Conteúdo não encontrado ({ccontent.Id} - {ccontent.Title})");

                        content.Title = ccontent.Title;
                        content.Excerpt = ccontent.Excerpt;

                        content.Duration = ccontent.Duration;
                        content.Type = ccontent.Type;
                        content.ReferenceUrls = new List<string>(ccontent.ReferenceUrls);
                        content.Concepts = ccontent.Concepts
                            .Select(x => new ConceptPosition(x.Name, x.Positions, x.Anchors)).ToList();
                        content.Value = ccontent.Value;
                        content.NumPages = ccontent.numPages;
                    }
                }

                await _db.ModuleCollection.ReplaceOneAsync(
                    t => t.Id == module.Id,
                    module,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(true);
            }

            private async Task<Module> GetModule(Contract request, CancellationToken cancellationToken)
            {
                var mId = ObjectId.Parse(request.ModuleId);
                var query = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken);
                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            }

            private Module DeleteNonExisting(Module module, List<ContractContent> contents)
            {
                foreach (var subject in module.Subjects)
                {
                    var requestIds = from content in contents
                                     where !string.IsNullOrEmpty(content.Id)
                                     select ObjectId.Parse(content.Id);

                    subject.Contents = subject.Contents.Where(
                        x => requestIds.Contains(x.Id)
                    ).ToList();
                }

                return module;
            }
        }
    }
}
