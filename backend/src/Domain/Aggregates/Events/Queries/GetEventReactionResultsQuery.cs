using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Events.EventReaction;

namespace Domain.Aggregates.Events.Queries
{
    public class GetEventReactionResultsQuery
    {
        public class Contract : CommandContract<Result<ReactionResults>>
        {
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string UserRole { get; set; }
        }

        public class ReactionResults
        {
            public string EventName { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public RatingItem Didactic { get; set; }
            public RatingItem ClassroomContent { get; set; }
            public RatingItem StudyContent { get; set; }
            public RatingItem TheoryAndPractice { get; set; }
            public RatingItem UsedResources { get; set; }
            public RatingItem EvaluationFormat { get; set; }
            public ExpectationItem Expectation { get; set; }
            public long StudentsCount { get; set; }
            public int ItemsCount { get; set; }
            public List<SuggestionItem> Suggestions { get; set; }
            public bool CanApprove { get; set; }
        }

        public class EventReactionItem
        {
            public ObjectId Id { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public ReactionRating Didactic { get; set; }
            public ReactionRating ClassroomContent { get; set; }
            public ReactionRating StudyContent { get; set; }
            public ReactionRating TheoryAndPractice { get; set; }
            public ReactionRating UsedResources { get; set; }
            public ReactionRating EvaluationFormat { get; set; }
            public ReactionExpectation Expectation { get; set; }
            public string Suggestions { get; set; }
            public bool Approved { get; set; }
        }

        public class RatingItem
        {
            public decimal Bad { get; set; }
            public decimal Unsatisfactory { get; set; }
            public decimal Satisfactory { get; set; }
            public decimal Good { get; set; }
            public decimal Excelent { get; set; }
        }

        public class ExpectationItem
        {
            public decimal BelowExpectation { get; set; }
            public decimal AsExpected { get; set; }
            public decimal ExceedExpectation { get; set; }
        }

        public class SuggestionItem
        {
            public ObjectId EventReactionId { get; set; }
            public string Suggestion { get; set; }
            public bool Approved { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ReactionResults>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result<ReactionResults>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.EventId))
                        return Result.Fail<ReactionResults>("Id do Evento não informado");
                    if (String.IsNullOrEmpty(request.EventScheduleId))
                        return Result.Fail<ReactionResults>("Id do Schedule não informado");

                    var reactions = await GetReactions(request, cancellationToken);
                    return Result.Ok( reactions );
                }
                catch (Exception err)
                {
                    return Result.Fail<ReactionResults>(
                        $"Ocorreu um erro ao buscar os resultados: {err.Message}"
                    );
                }
            }

            private async Task<ReactionResults> GetReactions(Contract request, CancellationToken token)
            {
                var scheduleId = ObjectId.Parse(request.EventScheduleId);

                //return _db.EventReactionCollection
                //    .AsQueryable()
                //    .Where(x => x.EventScheduleId == scheduleId)
                //    .GroupBy(x => x.EventScheduleId)
                //    .Select(x => new {
                //        Didact = new {
                //            Bad = x.Count(y => y.Didactic == ReactionRating.Bad),
                //            Unsatisfactory = x.Count(y => y.Didactic == ReactionRating.Unsatisfactory),
                //            Satisfactory = x.Count(y => y.Didactic == ReactionRating.Satisfactory),
                //            Good = x.Count(y => y.Didactic == ReactionRating.Good),
                //            Excelent = x.Count(y => y.Didactic == ReactionRating.Excelent)
                //        }
                //    })
                //    .FirstOrDefault();

                var query = await _db.Database
                        .GetCollection<EventReactionItem>("EventReactions")
                        .FindAsync(x =>
                            x.EventScheduleId == scheduleId,
                            cancellationToken: token
                        );
                
                var reactions = await query.ToListAsync(token);

                var eventId = ObjectId.Parse(request.EventId);
                var reactEvent = await GetEvent(eventId, token);

                return new ReactionResults
                {
                    EventName = reactEvent.Title,
                    EventDate = reactEvent.Schedules.First(
                            x => x.Id == scheduleId
                        ).EventDate,
                    Didactic = GetDidacticRating(reactions),
                    ClassroomContent = GetClassroomContentRating(reactions),
                    EvaluationFormat = GetEvaluationFormatRating(reactions),
                    StudyContent = GetStudyContentRating(reactions),
                    TheoryAndPractice = GetTheoryAndPracticeRating(reactions),
                    UsedResources = GetUsedResourcesRating(reactions),
                    Expectation = GetExpectationRating(reactions),
                    ItemsCount = reactions.Count,
                    StudentsCount = await CountApplications(scheduleId, token),
                    Suggestions = GetSuggestions(reactions, request),
                    CanApprove = request.UserRole == "Admin"
                };
            }

            private async Task<Event> GetEvent(ObjectId eventId, CancellationToken token)
            {
                var query = await _db.Database
                        .GetCollection<Event>("Events")
                        .FindAsync(x => x.Id == eventId,
                            cancellationToken: token
                        );

                return await query.FirstOrDefaultAsync(token);
            }

            private async Task<long> CountApplications(ObjectId scheduleId, CancellationToken token)
            {
                return await _db.Database
                    .GetCollection<EventApplication>("EventApplications")
                    .CountDocumentsAsync(x =>
                        x.ScheduleId == scheduleId,
                        cancellationToken: token
                    );
            }

            private List<SuggestionItem> GetSuggestions(List<EventReactionItem> reactions, Contract request)
            {
                var fReactions = reactions;

                if (request.UserRole != "Admin")
                    fReactions = fReactions.Where(r => r.Approved).ToList();

                return fReactions
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .Select(x => new SuggestionItem {
                            EventReactionId = x.Id,
                            Approved = x.Approved,
                            Suggestion = x.Suggestions
                        })
                        .ToList();
            }

            private RatingItem GetDidacticRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.Didactic == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.Didactic == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.Didactic == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.Didactic == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.Didactic == ReactionRating.Excelent)
                };
            }

            private RatingItem GetClassroomContentRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.ClassroomContent == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.ClassroomContent == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.ClassroomContent == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.ClassroomContent == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.ClassroomContent == ReactionRating.Excelent)
                };
            }

            private RatingItem GetStudyContentRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.StudyContent == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.StudyContent == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.StudyContent == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.StudyContent == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.StudyContent == ReactionRating.Excelent)
                };
            }

            private RatingItem GetTheoryAndPracticeRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.TheoryAndPractice == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.TheoryAndPractice == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.TheoryAndPractice == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.TheoryAndPractice == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.TheoryAndPractice == ReactionRating.Excelent)
                };
            }

            private RatingItem GetUsedResourcesRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.UsedResources == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.UsedResources == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.UsedResources == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.UsedResources == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.UsedResources == ReactionRating.Excelent)
                };
            }

            private RatingItem GetEvaluationFormatRating(List<EventReactionItem> reactions)
            {
                return new RatingItem
                {
                    Bad = reactions.Count(x => x.EvaluationFormat == ReactionRating.Bad),
                    Unsatisfactory = reactions.Count(x => x.EvaluationFormat == ReactionRating.Unsatisfactory),
                    Satisfactory = reactions.Count(x => x.EvaluationFormat == ReactionRating.Satisfactory),
                    Good = reactions.Count(x => x.EvaluationFormat == ReactionRating.Good),
                    Excelent = reactions.Count(x => x.EvaluationFormat == ReactionRating.Excelent)
                };
            }

            private ExpectationItem GetExpectationRating(List<EventReactionItem> reactions)
            {
                return new ExpectationItem
                {
                    BelowExpectation = reactions.Count(x => x.Expectation == ReactionExpectation.BelowExpectation),
                    AsExpected = reactions.Count(x => x.Expectation == ReactionExpectation.AsExpected),
                    ExceedExpectation = reactions.Count(x => x.Expectation == ReactionExpectation.ExceedExpectation)
                };
            }
        }
    }
}
