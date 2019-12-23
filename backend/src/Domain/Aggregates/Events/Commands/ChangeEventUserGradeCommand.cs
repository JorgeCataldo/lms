using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ChangeEventUserGradeCommand
    {
        public class Contract : CommandContract<Result> {
            public string EventApplicationId { get; set; }
            public decimal? OrganicGrade { get; set; }
            public decimal? InorganicGrade { get; set; }
            public List<CustomEventGradeValue> CustomEventGradeValues { get; set; }
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

                if (request.OrganicGrade == null || request.InorganicGrade == null)
                    return Result.Fail("Notas não informadas");
                else if (request.OrganicGrade.Value < 0 || request.InorganicGrade.Value < 0)
                    return Result.Fail("As notas devem ser valores positivos");

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

                application.OrganicGrade = request.OrganicGrade.Value;
                application.InorganicGrade = request.InorganicGrade.Value;
                if (application.CustomEventGradeValues == null)
                    application.CustomEventGradeValues = new List<CustomEventGradeValue>();

                foreach (CustomEventGradeValue incomingGrade in request.CustomEventGradeValues)
                {
                    var baseGrade = application.CustomEventGradeValues.FirstOrDefault(x => x.Key == incomingGrade.Key);
                    if (baseGrade  != null)
                    {
                        baseGrade.Grade = incomingGrade.Grade;
                    }
                    else
                    {
                        application.CustomEventGradeValues.Add(incomingGrade);
                    }
                }

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
