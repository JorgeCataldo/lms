using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ChangeEventUserForumGradeCommand
    {
        public class Contract : CommandContract<Result> {
            public string EventApplicationId { get; set; }
            public decimal? ForumGrade { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>{
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.EventApplicationId))
                    return Result.Fail("Id da Inscrição não informado");

                if (request.ForumGrade == null)
                    return Result.Fail("Nota não informada");
                else if (request.ForumGrade.Value < 0)
                    return Result.Fail("A nota deve ter valor positivo");

                var applicationId = ObjectId.Parse(request.EventApplicationId);

                var query = await _db.EventApplicationCollection
                    .FindAsync(u =>
                        u.Id == applicationId,
                        cancellationToken: cancellationToken
                    );

                var application = await query.SingleOrDefaultAsync(cancellationToken);
                if (application == null)
                    return Result.Fail("Inscrição não existe");

                if (request.UserRole == "Student")
                {
                    var dbEvent = await GetEvent(application.EventId, cancellationToken);
                    var userId = ObjectId.Parse(request.UserId);

                    if (!dbEvent.InstructorId.HasValue || dbEvent.InstructorId != userId)
                        return Result.Fail("Acesso Negado");
                }

                application.ForumGrade = request.ForumGrade.Value;

                await _db.EventApplicationCollection.ReplaceOneAsync(t =>
                    t.Id == applicationId, application,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }

            private async Task<Event> GetEvent(ObjectId eventId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Event>("Event")
                    .FindAsync(x => x.Id == eventId,
                        cancellationToken: token
                    );

                return await query.FirstOrDefaultAsync(token);
            }
        }
    }
}
