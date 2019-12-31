using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums.Commands
{
    public class RemoveEventForumAnswerCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string AnswerId { get; set; }
            public string EventScheduleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.AnswerId))
                        return Result.Fail("Id da resposta não informado");

                    if (request.UserRole == "Student")
                    {
                        ObjectId userId = ObjectId.Parse(request.UserId);
                        ObjectId eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                        bool isInstructor = await CheckEventInstructor(eventScheduleId, userId, cancellationToken);
                        if (!isInstructor)
                            return Result.Fail<bool>("Acesso Negado");
                    }

                    var answer = await GetAnswer(request.AnswerId, cancellationToken);
                    if (answer == null)
                        return Result.Fail("Resposta não existe");

                    await _db.EventForumAnswerCollection.DeleteOneAsync(fA =>
                        fA.Id == answer.Id,
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw err;
                }
            }

            private async Task<EventForumAnswer> GetAnswer(string rAnswerId, CancellationToken cancellationToken)
            {
                var answerId = ObjectId.Parse(rAnswerId);

                var query = await _db.Database
                    .GetCollection<EventForumAnswer>("EventForumAnswers")
                    .FindAsync(
                        x => x.Id == answerId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private async Task<bool> CheckEventInstructor(
                ObjectId eventScheduleId, ObjectId userId, CancellationToken cancellationToken
            )
            {
                var query = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x =>
                        x.Schedules.Any(y => y.Id == eventScheduleId) &&
                        x.InstructorId.HasValue &&
                        x.InstructorId.Value == userId,
                        cancellationToken: cancellationToken
                    );
                return await query.AnyAsync();
            }
        }
    }
}
