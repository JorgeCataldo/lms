using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using Domain.ECommerceIntegration.Woocommerce;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class PublishModuleDraftCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
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

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                
                if (request.UserRole != "Secretary" && request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail("Acesso Negado");

                var modId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);

                var draft = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == modId || x.ModuleId == modId
                        )
                    )
                    .FirstOrDefaultAsync();

                if (draft == null)
                    return Result.Fail("Não há rascunho para ser publicado.");

                if (request.UserRole == "Author" && draft.CreatedBy != userId)
                    return Result.Fail("Você não tem permissão para publicar o módulo selecionado.");

                bool updated = await UpdateCurrentModule(userId, draft, cancellationToken);
                if (!updated)
                    return Result.Fail("Módulo não existe.");

                draft.PublishDraft();

                await _db.ModuleDraftCollection.ReplaceOneAsync(t =>
                    t.Id == draft.Id, draft,
                    cancellationToken: cancellationToken
                );

                var questionDrafts = await _db.QuestionDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.DraftId == draft.Id
                        )
                    )
                    .ToListAsync();

                await _db.QuestionCollection.DeleteManyAsync(x =>
                    x.ModuleId == draft.ModuleId,
                    cancellationToken: cancellationToken
                );

                if (questionDrafts.Count > 0)
                {
                    IEnumerable<Question> newQuestions;

                    newQuestions = questionDrafts.Select(q =>
                        Question.Create(
                            q.QuestionId, q.Text, q.Level, q.Duration,
                            q.Concepts, q.Answers,
                            q.ModuleId, q.SubjectId
                        ).Data
                    );

                    await _db.QuestionCollection.InsertManyAsync(
                        newQuestions, cancellationToken: cancellationToken
                    );
                }

                var update = Builders<QuestionDraft>.Update.Set(d => d.DraftPublished, true);
                var ids = questionDrafts.Select(x => x.Id).ToArray();

                await _db.QuestionDraftCollection.UpdateManyAsync(x =>
                    ids.Contains(x.Id), update,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }

            private async Task<bool> UpdateCurrentModule(ObjectId userId, ModuleDraft draft, CancellationToken token) {
                var module = await _db.ModuleCollection.AsQueryable()
                    .Where(mod => mod.Id == draft.ModuleId)
                    .FirstOrDefaultAsync();

                if (module == null)
                    return false;

                string config = _configuration[$"EcommerceIntegration:Active"];

                if (!draft.EcommerceId.HasValue && draft.CreateInEcommerce && config == "True")
                {
                    var response = await CreateEcommerceProduct(module);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var parsed = JObject.Parse(content);
                        module.EcommerceId = (long)parsed["product"]["id"];
                    }
                    else
                    {
                        var error = ErrorLog.Create(
                            ErrorLogTypeEnum.EcommerceIntegration,
                            "create-module", content
                        ).Data;

                        await _db.ErrorLogCollection.InsertOneAsync(
                            error, cancellationToken: token
                        );
                    }
                }

                var serializedModuleOldValues = JsonConvert.SerializeObject(new List<Module>
                {
                    module
                });

                module.Title = draft.Title;
                module.Excerpt = draft.Excerpt;
                module.ImageUrl = draft.ImageUrl;
                module.Published = draft.Published;
                module.InstructorId = draft.InstructorId;
                module.ExtraInstructorIds = draft.ExtraInstructorIds;
                module.Instructor = draft.Instructor;
                module.InstructorMiniBio = draft.InstructorMiniBio;
                module.InstructorImageUrl = draft.InstructorImageUrl;
                module.Duration = draft.Duration;
                module.VideoUrl = draft.VideoUrl;
                module.VideoDuration = draft.VideoDuration;
                module.CertificateUrl = draft.CertificateUrl;
                module.StoreUrl = draft.StoreUrl;
                module.EcommerceUrl = draft.EcommerceUrl;
                module.Tags = draft.Tags;
                module.TutorsIds = draft.TutorsIds;
                module.Subjects = draft.Subjects;
                module.Requirements = draft.Requirements;
                module.SupportMaterials = draft.SupportMaterials;
                module.EcommerceId = draft.EcommerceId;
                module.ModuleGradeType = draft.ModuleGradeType;
                var listEcomerce = new List<EcommerceModule>();
                if (draft.EcommerceProducts != null  && draft.EcommerceProducts.Count > 0)
                {
                    for (int i = 0; i < draft.EcommerceProducts.Count; i++)
                    {
                        var currentDraftProduct = draft.EcommerceProducts[i];
                        listEcomerce.Add(new EcommerceModule()
                        {
                            EcommerceId = currentDraftProduct.EcommerceId,
                            UsersAmount = currentDraftProduct.UsersAmount,
                            DisableEcommerce = currentDraftProduct.DisableEcommerce,
                            Price = currentDraftProduct.Price,
                            DisableFreeTrial = currentDraftProduct.DisableFreeTrial,
                            LinkEcommerce = currentDraftProduct.LinkEcommerce,
                            LinkProduct = currentDraftProduct.LinkProduct,
                            Subject = currentDraftProduct.Subject,
                            Hours = currentDraftProduct.Hours
                        });
                    }
                }
                module.EcommerceProducts = listEcomerce;

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: token
                );

                var newDraftList = new List<Module>
                {
                    module
                };

                var auditLog = AuditLog.Create(userId, module.Id, module.GetType().ToString(), 
                    JsonConvert.SerializeObject(newDraftList), EntityAction.Update, serializedModuleOldValues);
                await _db.AuditLogCollection.InsertOneAsync(auditLog);

                return true;
            }

            private async Task<HttpResponseMessage> CreateEcommerceProduct(Module module)
            {
                var product = new EcommerceItem {
                    product = new ProductItem {
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
