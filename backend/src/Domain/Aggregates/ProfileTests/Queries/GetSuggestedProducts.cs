using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestResponse;
using static Domain.Aggregates.ProfileTests.SuggestedProduct;

namespace Domain.Aggregates.ProfileTests.Queries
{
    public class GetSuggestedProductsQuery
    {
        public class Contract : CommandContract<Result<SuggestedProducts>>
        {
            public string UserId { get; set; }
        }

        public class SuggestedProducts
        {
            public List<SuggestionItem> Modules { get; set; }
            public List<SuggestionItem> Events { get; set; }
            public List<SuggestionItem> Tracks { get; set; }
            public SuggestedTestItem Test { get; set; }
            public SuggestedTestItem DefaultTest { get; set; }
        }

        public class SuggestionItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public SuggestedProductType? Type { get; set; }
            public string StoreUrl { get; set; }
        }

        public class SuggestedTestItem
        {
            public ObjectId CreatedBy { get; set; }
            public ObjectId TestId { get; set; }
            public string TestTitle { get; set; }
            public List<ProfileTestAnswer> Answers { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<SuggestedProducts>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<SuggestedProducts>> Handle(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);

                var suggestions = await _db.SuggestedProductCollection.AsQueryable()
                    .Where(s => s.UserId == userId)
                    .ToListAsync(token);

                var test = await GetSuggestedTest(userId, token);
                SuggestedTestItem defaultTest = null;

                if (test == null)
                    defaultTest = await GetDefaultTest(userId, token);

                return Result.Ok(
                    new SuggestedProducts {
                        Modules = await GetModules(suggestions, token),
                        Events = await GetEvents(suggestions, token),
                        Tracks = await GetTracks(suggestions, token),
                        Test = test,
                        DefaultTest = defaultTest
                    }
                );
            }

            public async Task<SuggestedTestItem> GetSuggestedTest(ObjectId userId, CancellationToken token)
            {
                return await _db.ProfileTestResponseCollection.AsQueryable()
                    .Where(s =>
                        s.CreatedBy == userId
                    )
                    .Select(x => new SuggestedTestItem {
                        TestId = x.TestId,
                        TestTitle = x.TestTitle,
                        CreatedBy = x.CreatedBy,
                        Answers = x.Answers
                    })
                    .FirstOrDefaultAsync(token);
            }

            public async Task<SuggestedTestItem> GetDefaultTest(ObjectId userId, CancellationToken token)
            {
                return await _db.ProfileTestCollection.AsQueryable()
                    .Where(s => s.IsDefault)
                    .Select(x => new SuggestedTestItem {
                        TestId = x.Id,
                        TestTitle = x.Title
                    })
                    .FirstOrDefaultAsync(token);
            }

            public async Task<List<SuggestionItem>> GetModules(List<SuggestedProduct> suggestions, CancellationToken token)
            {
                var modulesIds = suggestions
                    .Where(s => s.Type == SuggestedProductType.Module)
                    .Select(s => s.ProductId);

                var modules = await _db.ModuleCollection.AsQueryable()
                    .Where(m => modulesIds.Contains(m.Id))
                    .ToListAsync(token);

                return modules.Select(x => new SuggestionItem {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    Type = SuggestedProductType.Module,
                    StoreUrl = x.StoreUrl
                }).ToList();
            }

            public async Task<List<SuggestionItem>> GetEvents(List<SuggestedProduct> suggestions, CancellationToken token)
            {
                var eventsIds = suggestions
                    .Where(s => s.Type == SuggestedProductType.Event)
                    .Select(s => s.ProductId);

                var events = await _db.EventCollection.AsQueryable()
                    .Where(m => eventsIds.Contains(m.Id))
                    .ToListAsync(token);

                return events.Select(x => new SuggestionItem {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    Type = SuggestedProductType.Event,
                    StoreUrl = x.StoreUrl
                }).ToList();
            }

            public async Task<List<SuggestionItem>> GetTracks(List<SuggestedProduct> suggestions, CancellationToken token)
            {
                var tracksIds = suggestions
                    .Where(s => s.Type == SuggestedProductType.Track)
                    .Select(s => s.ProductId);

                var tracks = await _db.TrackCollection.AsQueryable()
                    .Where(m => tracksIds.Contains(m.Id))
                    .ToListAsync(token);

                return tracks.Select(x => new SuggestionItem {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    Type = SuggestedProductType.Track,
                    StoreUrl = x.StoreUrl
                }).ToList();
            }
        }
    }
}
