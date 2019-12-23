using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.ECommerceIntegration.Woocommerce;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;
using Performance.Domain.Aggregates.AuditLogs;
using Newtonsoft.Json;

namespace Domain.Aggregates.Events.Commands
{
    public class AddOrModifyEventCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public int? Duration { get; set; }
            public string[] Tags { get; set; }
            public PrepQuiz PrepQuiz { get; set; }
            public List<PrepQuizItem> PrepQuizQuestionList { get; set; }
            public string[] PrepQuizQuestions { get; set; }
            public string CertificateUrl { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public List<string> TutorsIds { get; set; }
            public string StoreUrl { get; set; }
            public bool? CreateInEcommerce { get; set; } = false;
            public long? EcommerceId { get; set; }
            public bool? ForceProblemStatement { get; set; } = false;
        }

        public class PrepQuizItem
        {
            public int Id { get; set; }
            public string Question { get; set; }
            public bool FileAsAnswer { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
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

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                ObjectId instructorId = String.IsNullOrEmpty(request.InstructorId) ? ObjectId.Empty : ObjectId.Parse(request.InstructorId);
                var tutorsIds = request.TutorsIds != null ? request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList() :
                        new List<ObjectId>();

                var eventResult = Event.Create(
                    request.Title,
                    request.Excerpt,
                    request.ImageUrl,
                    instructorId,
                    request.Instructor,
                    request.InstructorMiniBio,
                    request.InstructorImageUrl,
                    request.Tags,
                    request.VideoUrl,
                    request.VideoDuration,
                    request.Duration,
                    request.CertificateUrl,
                    tutorsIds,
                    request.StoreUrl,
                    request.ForceProblemStatement
                );

                if (eventResult.IsFailure)
                    return Result.Fail<Contract>(eventResult.Error);

                var newEvent = eventResult.Data;
                newEvent.PrepQuizQuestionList = new List<PrepQuizQuestion>();

                if (request.PrepQuizQuestionList != null && request.PrepQuizQuestionList.Count > 0)
                {
                    var questions = request.PrepQuizQuestionList.Select(x => x.Question).ToArray();

                    for (int i = 0; i < request.PrepQuizQuestionList.Count; i++)
                    {
                        var prepQuizResult = PrepQuizQuestion.Create(request.PrepQuizQuestionList[i].Question, request.PrepQuizQuestionList[i].FileAsAnswer, questions);
                        if (prepQuizResult.IsFailure)
                            return Result.Fail<Contract>(prepQuizResult.Error);
                        newEvent.PrepQuizQuestionList.Add(prepQuizResult.Data);
                    }
                }

                if (string.IsNullOrEmpty(request.Id))
                {
                    if (request.UserRole == "Student")
                        return Result.Fail<Contract>("Acesso Negado");

                    string config = _configuration[$"EcommerceIntegration:Active"];

                    if (request.CreateInEcommerce.HasValue && request.CreateInEcommerce.Value && config == "True")
                    {
                        var response = await CreateEcommerceProduct(newEvent);
                        string content = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            var parsed = JObject.Parse(content);
                            newEvent.EcommerceId = (long)parsed["product"]["id"];
                        }
                        else
                        {
                            var error = ErrorLog.Create(
                                ErrorLogTypeEnum.EcommerceIntegration,
                                "create-track", content
                            ).Data;

                            await _db.ErrorLogCollection.InsertOneAsync(
                                error, cancellationToken: cancellationToken
                            );
                        }
                    }

                    var newEventList = new List<Event>
                    {
                        newEvent
                    };

                    await _db.EventCollection.InsertOneAsync(
                        newEvent,
                        cancellationToken: cancellationToken
                    );

                    var creationLog = AuditLog.Create(ObjectId.Parse(request.UserId), newEvent.Id, newEvent.GetType().ToString(),
                    JsonConvert.SerializeObject(newEventList), EntityAction.Add);

                    await _db.AuditLogCollection.InsertOneAsync(creationLog);

                    request.Id = newEvent.Id.ToString();
                    request.EcommerceId = newEvent.EcommerceId;
                }
                else
                {
                    var evt = await (await _db
                            .Database
                            .GetCollection<Event>("Events")
                            .FindAsync(x => x.Id == ObjectId.Parse(request.Id), cancellationToken: cancellationToken))
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (evt == null)
                        return Result.Fail<Contract>("Evento não Encontrado");

                    if (request.UserRole == "Student")
                    {
                        var userId = ObjectId.Parse(request.UserId);

                        if (!evt.InstructorId.HasValue || evt.InstructorId.Value != userId)
                            return Result.Fail<Contract>("Acesso Negado");
                    }

                    var oldEventList = JsonConvert.SerializeObject(new List<Event>
                    {
                        newEvent
                    });

                    evt.Title = newEvent.Title;
                    evt.Excerpt = newEvent.Excerpt;
                    evt.ImageUrl = newEvent.ImageUrl;
                    evt.InstructorId = newEvent.InstructorId;
                    evt.Instructor = newEvent.Instructor;
                    evt.InstructorMiniBio = newEvent.InstructorMiniBio;
                    evt.InstructorImageUrl = newEvent.InstructorImageUrl;
                    evt.Tags = newEvent.Tags;
                    evt.VideoUrl = newEvent.VideoUrl;
                    evt.VideoDuration = newEvent.VideoDuration;
                    evt.Duration = newEvent.Duration;
                    evt.PrepQuiz = newEvent.PrepQuiz;
                    evt.CertificateUrl = newEvent.CertificateUrl;
                    evt.TutorsIds = newEvent.TutorsIds;
                    evt.StoreUrl = newEvent.StoreUrl;
                    evt.EcommerceId = request.EcommerceId;
                    evt.PrepQuizQuestionList = newEvent.PrepQuizQuestionList;
                    evt.ForceProblemStatement = newEvent.ForceProblemStatement;

                    await _db.EventCollection.ReplaceOneAsync(t =>
                        t.Id == evt.Id, evt,
                        cancellationToken: cancellationToken
                    );

                    var newEventList = new List<Event>
                    {
                        evt
                    };

                    var changeLog = AuditLog.Create(ObjectId.Parse(request.UserId), evt.Id, evt.GetType().ToString(),
                    JsonConvert.SerializeObject(newEventList), EntityAction.Update, oldEventList);

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);

                    if (evt.EcommerceId.HasValue)
                    {
                        string config = _configuration[$"EcommerceIntegration:Active"];
                        if (config == "True")
                            await UpdateEcommerceProduct(evt);
                    }
                }

                return Result.Ok(request);
            }

            private async Task<HttpResponseMessage> CreateEcommerceProduct(Event dbEvent)
            {
                var product = new EcommerceItem
                {
                    product = new ProductItem
                    {
                        visible = false,
                        title = dbEvent.Title,
                        type = "simple",
                        regular_price = "0",
                        description = dbEvent.Excerpt
                    }
                };

                return await Woocommerce.CreateProduct(
                    product, _configuration
                );
            }

            private async Task<HttpResponseMessage> UpdateEcommerceProduct(Event dbEvent)
            {
                var product = new EcommerceItem
                {
                    product = new ProductItem
                    {
                        id = dbEvent.EcommerceId,
                        title = dbEvent.Title
                    }
                };

                return await Woocommerce.UpdateProduct(
                    product, _configuration
                );
            }
        }
    }
}
