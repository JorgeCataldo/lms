using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Events.EventReaction;

namespace Domain.Aggregates.Events.Commands
{
    public class AddEventReactionCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string UserId { get; set; }
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
            public int Didactic { get; set; }
            public int ClassroomContent { get; set; }
            public int StudyContent { get; set; }
            public int TheoryAndPractice { get; set; }
            public int UsedResources { get; set; }
            public int EvaluationFormat { get; set; }
            public int Expectation { get; set; }
            public string Suggestions { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var eventReactionResult = Create(
                    ObjectId.Parse(request.EventId),
                    ObjectId.Parse(request.EventScheduleId),
                    ObjectId.Parse(request.UserId),
                    (ReactionRating)request.Didactic,
                    (ReactionRating)request.ClassroomContent,
                    (ReactionRating)request.StudyContent,
                    (ReactionRating)request.TheoryAndPractice,
                    (ReactionRating)request.UsedResources,
                    (ReactionRating)request.EvaluationFormat,
                    (ReactionExpectation)request.Expectation,
                    request.Suggestions
                );
                
                if (eventReactionResult.IsFailure)
                    return Result.Fail<Contract>(eventReactionResult.Error);

                var evt = await _db.EventCollection.AsQueryable()
                    .Where(x => x.Id == eventReactionResult.Data.EventId)
                    .FirstOrDefaultAsync();

                if(evt == null || !evt.Schedules.Any(x=>x.Id == eventReactionResult.Data.EventScheduleId))
                    return Result.Fail<Contract>("Evento não encontrado");

                var schedule = evt.Schedules.First(x => x.Id == eventReactionResult.Data.EventScheduleId);

                if((DateTime.Now - schedule.EventDate).TotalDays > 14)
                    return Result.Fail<Contract>("Evento ocorrido a mais de 14 dias. Não aceita-se mais avaliações");

                var existingReaction = await _db.EventReactionCollection.AsQueryable()
                    .Where(x => x.CreatedBy == eventReactionResult.Data.CreatedBy &&
                        x.EventId == eventReactionResult.Data.EventId &&
                        x.EventScheduleId == eventReactionResult.Data.EventScheduleId)
                    .FirstOrDefaultAsync();

                if (existingReaction != null)
                {
                    await _db.EventReactionCollection.DeleteOneAsync(x => x.Id == existingReaction.Id, cancellationToken);
                }

                await _db.EventReactionCollection.InsertOneAsync(eventReactionResult.Data, cancellationToken: cancellationToken);
               
                return Result.Ok(request);
            }
        }
    }
}
