using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.ECommerceIntegration.Woocommerce;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class AddModuleCommand
    {
        public class Contract : CommandContract<Result<Module>>
        {
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public bool Published { get; set; }
            public int? Duration { get; set; }
            public List<string> Tags { get; set; }
            public List<ContractSubject> Subjects { get; set; }
            public string UserRole { get; set; }
            public string CertificateUrl { get; set; }
            public List<string> TutorsIds { get; set; }
            public List<string> ExtraInstructorIds { get; set; }
            public string StoreUrl { get; set; }
            public bool? CreateInEcommerce { get; set; }
            public int? ValidFor { get; set; }

            public Contract()
            {
                Tags = new List<string>();
                Subjects = new List<ContractSubject>();
            }
        }

        public class ContractSubject
        {
            public string[] Concepts { get; set; }
            public List<ContractContent> Contents { get; set; }

            public string Title { get; set; }
            public string Excerpt { get; set; }
        }

        public class ContractContent
        {
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public List<string> ReferenceUrls { get; set; }
            public string[] Concepts { get; set; }
            public string Link { get; set; }
            public ContentType Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Module>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IMediator mediator, IConfiguration configuration)
            {
                _db = db;
                _mediator = mediator;
                _configuration = configuration;
            }

            public async Task<Result<Module>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                        return Result.Fail<Module>("Acesso Negado");

                    Duration duration = null;
                    if (request.Duration.HasValue)
                    {
                        var durationResult = Duration.Create(request.Duration.Value);
                        if (durationResult.IsFailure)
                            return Result.Fail<Module>(durationResult.Error);

                        duration = durationResult.Data;
                    }

                    var tags = new List<Tag>();
                    foreach (var tagStr in request.Tags)
                    {
                        var tag = Tag.Create(tagStr);
                        tags.Add(tag.Data);
                    }

                    var tutorsIds = request.TutorsIds != null ? request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList() :
                        new List<ObjectId>();

                    var extraInstructorIds = request.ExtraInstructorIds != null ? request.ExtraInstructorIds.Select(x => ObjectId.Parse(x)).ToList() :
                        new List<ObjectId>();

                    var module = Module.Create(
                        request.Title, request.Excerpt, 
                        request.InstructorId, request.Instructor, request.InstructorMiniBio,
                        request.ImageUrl, request.InstructorImageUrl,
                        request.Published, duration, tags, request.CertificateUrl,
                        tutorsIds, extraInstructorIds, request.StoreUrl
                    );

                    if (module.IsFailure)
                        return Result.Fail<Module>(module.Error);
                    
                    if (request.Subjects != null)
                    {
                        foreach (var csubject in request.Subjects)
                        {
                            var subject = Subject.Create(csubject.Title, csubject.Excerpt, csubject.Concepts);
                            if (subject.IsFailure)
                                return Result.Fail<Module>(subject.Error);

                            module.Data.Subjects.Add(subject.Data);
                            foreach (var ccontent in csubject.Contents)
                            {
                                var content = Content.Create(
                                    ccontent.Title, ccontent.Excerpt,
                                    ccontent.Concepts.Select(x =>
                                        new ConceptPosition(x, null, null)
                                    ).ToList(), new string[] {},
                                    ccontent.Link,
                                    ccontent.Type, 0
                                );

                                if (content.IsFailure)
                                    return Result.Fail<Module>(subject.Error);

                                subject.Data.Contents.Add(content.Data);
                            }
                        }
                    }

                    string config = _configuration[$"EcommerceIntegration:Active"];

                    if (request.CreateInEcommerce.HasValue && request.CreateInEcommerce.Value && config == "True")
                    {
                        var response = await CreateEcommerceProduct(module.Data);
                        string content = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            var parsed = JObject.Parse(content);
                            module.Data.EcommerceId = (long)parsed["product"]["id"];
                        }
                        else
                        {
                            var error = ErrorLog.Create(
                                ErrorLogTypeEnum.EcommerceIntegration,
                                "create-module", content
                            ).Data;

                            await _db.ErrorLogCollection.InsertOneAsync(
                                error, cancellationToken: cancellationToken
                            );
                        }
                    }

                    await _db.ModuleCollection.InsertOneAsync(
                        module.Data,
                        cancellationToken: cancellationToken
                    );

                    return module;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw;
                }
            }

            private async Task<HttpResponseMessage> CreateEcommerceProduct(Module module)
            {
                var product = new EcommerceItem
                {
                    product = new ProductItem
                    {
                        visible = false,
                        title = module.Title,
                        type = "simple",
                        regular_price = "0",
                        description = module.Excerpt
                    }
                };

                return await Woocommerce.CreateProduct(
                    product, _configuration
                );
            }
        }
    }
}
