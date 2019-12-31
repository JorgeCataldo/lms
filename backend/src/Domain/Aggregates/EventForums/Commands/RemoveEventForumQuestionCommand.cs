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
    public class RemoveEventForumQuestionCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string QuestionId { get; set; }
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
                    if (String.IsNullOrEmpty(request.QuestionId))
                        return Result.Fail("Id da pergunta não informado");

                    if (request.UserRole == "Student")
                    {
                        ObjectId userId = ObjectId.Parse(request.UserId);
                        ObjectId eventScheduleId = ObjectId.Parse(request.EventScheduleId);

                        bool isInstructor = await CheckEventInstructor(eventScheduleId, userId, cancellationToken);
                        if (!isInstructor)
                            return Result.Fail<bool>("Acesso Negado");
                    }

                    var question = await GetQuestion(request.QuestionId, cancellationToken);
                    if (question == null)
                        return Result.Fail("Pergunta não existe");

                    await _db.EventForumQuestionCollection.DeleteOneAsync(fQ =>
                        fQ.Id == question.Id,
                        cancellationToken: cancellationToken
                    );

                    await _db.NotificationCollection.DeleteManyAsync(notification =>
                        notification.RedirectPath.Contains(
                            question.Id.ToString()
                        ),
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

            private async Task<EventForumQuestion> GetQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);

                var query = await _db.Database
                    .GetCollection<EventForumQuestion>("EventForumQuestions")
                    .FindAsync(
                        x => x.Id == questionId,
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
