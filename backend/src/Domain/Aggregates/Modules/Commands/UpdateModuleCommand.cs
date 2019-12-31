using System.Collections.Generic;
using System.Linq;
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
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class UpdateModuleCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string ImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public string InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public bool Published { get; set; }
            public int? Duration { get; set; }
            public List<string> Tags { get; set; }
            public string CertificateUrl { get; set; }
            public List<string> TutorsIds { get; set; }
            public List<string> ExtraInstructorIds { get; set; }
            public string StoreUrl { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
            public long? EcommerceId { get; set; }
            public int? ValidFor { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
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

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                var mId = ObjectId.Parse(request.Id);
                var module = await (await _db
                        .Database
                        .GetCollection<Module>("Modules")
                        .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                
                if (module == null)
                    return Result.Fail<bool>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if ((!Module.IsInstructor(module, userId).Data) && !module.TutorsIds.Contains(userId))
                        return Result.Fail<bool>("Acesso Negado");
                }

                Duration duration = null;
                if (request.Duration.HasValue)
                {
                    var durationResult = Duration.Create(request.Duration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail<bool>(durationResult.Error);

                    duration = durationResult.Data;
                }

                Duration videoDuration = null;
                if (request.VideoDuration.HasValue)
                {
                    var durationResult = Duration.Create(request.VideoDuration.Value);
                    if (durationResult.IsFailure)
                        return Result.Fail<bool>(durationResult.Error);

                    videoDuration = durationResult.Data;
                }

                if (request.TutorsIds == null)
                    request.TutorsIds = new List<string>();

                if (request.ExtraInstructorIds == null)
                    request.ExtraInstructorIds = new List<string>();

                module.Title = request.Title;
                module.Excerpt = request.Excerpt;
                module.InstructorId = string.IsNullOrEmpty(request.InstructorId) ? ObjectId.Empty : ObjectId.Parse(request.InstructorId);
                module.Instructor = request.Instructor;
                module.InstructorMiniBio = request.InstructorMiniBio;
                module.InstructorImageUrl = request.InstructorImageUrl;
                module.ImageUrl = request.ImageUrl;
                module.VideoUrl = request.VideoUrl;
                module.VideoDuration = videoDuration;
                module.Published = request.Published;
                module.Duration = duration;
                module.Tags = request.Tags.Select(
                    t => ValueObjects.Tag.Create(t).Data
                ).ToList();
                module.CertificateUrl = request.CertificateUrl;
                module.TutorsIds = request.TutorsIds.Select(x => ObjectId.Parse(x)).ToList();
                module.ExtraInstructorIds = request.ExtraInstructorIds.Select(x => ObjectId.Parse(x)).ToList();
                module.StoreUrl = request.StoreUrl;
                module.EcommerceId = request.EcommerceId;
                module.ValidFor = request.ValidFor;

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );

                if (module.EcommerceId.HasValue)
                {
                    string config = _configuration[$"EcommerceIntegration:Active"];
                    if (config == "True")
                        await UpdateEcommerceProduct(module);
                }

                return Result.Ok(true);
            }

            private async Task<HttpResponseMessage> UpdateEcommerceProduct(Module module)
            {
                var product = new EcommerceItem {
                    product = new ProductItem {
                        id = module.EcommerceId,
                        title = module.Title
                    }
                };

                return await Woocommerce.UpdateProduct(
                    product, _configuration
                );
            }
        }
    }
}
