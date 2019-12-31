using System;
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

namespace Domain.Aggregates.Events.Commands
{
    public class ManageEventReactionSuggestionCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string EventReactionId { get; set; }
            public bool Approved { get; set; }
            public string UserRole { get; set; }
        }

        public class EventReactionItem
        {
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
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;

            public Handler(IDbContext db, IMediator mediator, UserManager<User> userManager)
            {
                _db = db;
                _userManager = userManager;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.EventReactionId))
                        return Result.Fail("Id da Avaliação de Reação não informado");

                    if (request.UserRole == "Student" || request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                        return Result.Fail<UnauthorizedAccessException>("Acesso negado");

                    var reaction = await GetReaction(request.EventReactionId, cancellationToken);

                    reaction.Approved = request.Approved;

                    await _db.EventReactionCollection.ReplaceOneAsync(r =>
                        r.Id == reaction.Id, reaction,
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok();
                }
                catch (Exception err)
                {
                    return Result.Fail(
                        $"Ocorreu um erro ao buscar os resultados: {err.Message}"
                    );
                }
            }

            private async Task<EventReaction> GetReaction(string rReactionId, CancellationToken token)
            {
                var reactionId = ObjectId.Parse(rReactionId);

                var query = await _db.Database
                        .GetCollection<EventReaction>("EventReactions")
                        .FindAsync(x => x.Id == reactionId,
                            cancellationToken: token
                        );

                return await query.FirstOrDefaultAsync(token);
            }
        }
    }
}
