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
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class ManageUserPresenceCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string EventApplicationId { get; set; }
            public bool Presence { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
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
                    if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                        return Result.Fail("Acesso Negado");

                    if (String.IsNullOrEmpty(request.EventApplicationId))
                        return Result.Fail("Id da Inscrição no Evento não informado");

                    var application = await GetApplication(request.EventApplicationId, cancellationToken);

                    if (application == null)
                        return Result.Fail("Inscrição no Evento não existe");

                    if (request.UserRole == "Student")
                    {
                        var dbEvent = await GetEvent(application.EventId, cancellationToken);
                        var userId = ObjectId.Parse(request.UserId);

                        if ((dbEvent.InstructorId.HasValue && dbEvent.InstructorId.Value != userId) && !dbEvent.TutorsIds.Contains(userId))
                            return Result.Fail("Acesso Negado");
                    }           

                    application.UserPresence = request.Presence;

                    await _db.EventApplicationCollection.ReplaceOneAsync(r =>
                        r.Id == application.Id, application,
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok();
                }
                catch (Exception err)
                {
                    return Result.Fail(
                        $"Ocorreu um erro ao atualizar presença do aluno: {err.Message}"
                    );
                }
            }

            private async Task<EventApplication> GetApplication(string rApplicationId, CancellationToken token)
            {
                var applicationId = ObjectId.Parse(rApplicationId);

                var query = await _db.Database
                        .GetCollection<EventApplication>("EventApplications")
                        .FindAsync(x => x.Id == applicationId,
                            cancellationToken: token
                        );

                return await query.FirstOrDefaultAsync(token);
            }

            private async Task<Event> GetEvent(ObjectId eventId, CancellationToken token)
            {
                return await _db.EventCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == eventId);
            }
        }
    }
}
