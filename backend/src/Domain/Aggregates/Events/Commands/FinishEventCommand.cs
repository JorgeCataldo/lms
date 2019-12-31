using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using Domain.Extensions;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Commands
{
    public class FinishEventCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public List<string> UsersId { get; set; }
            public string EventId { get; set; }
            public string EventScheduleId { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;
            private readonly IEmailProvider _provider;
            private readonly IOptions<DomainOptions> _options;

            public Handler(IDbContext db, UserManager<User> userManager, 
                IEmailProvider provider, IOptions<DomainOptions> options)
            {
                _db = db;
                _userManager = userManager;
                _provider = provider;
                _options = options;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                var eventQry = await _db.EventCollection.FindAsync(u => u.Id == ObjectId.Parse(request.EventId), cancellationToken: cancellationToken);
                var eventDb = await eventQry.SingleOrDefaultAsync(cancellationToken);

                if (eventDb != null)
                {
                    if (request.UserRole == "Student")
                    {
                        var userId = ObjectId.Parse(request.UserId);

                        if (!eventDb.InstructorId.HasValue || eventDb.InstructorId.Value != userId)
                            return Result.Fail<Contract>("Acesso Negado");
                    }

                    var eventSchedule = eventDb.Schedules.Find(x => x.Id == ObjectId.Parse(request.EventScheduleId));

                    if (eventSchedule != null)
                    {
                        eventSchedule.FinishedAt = DateTimeOffset.Now;
                        eventSchedule.FinishedBy = ObjectId.Parse(request.UserId);
                        await _db.EventCollection.ReplaceOneAsync(t => t.Id == eventDb.Id, eventDb,
                           cancellationToken: cancellationToken);
                        if (!eventSchedule.SentReactionEvaluationEmails)
                        {
                            if (!request.UsersId.IsNullOrEmpty())
                            {
                                foreach (string userId in request.UsersId)
                                {
                                    await SendEmailToUser(ObjectId.Parse(userId), eventDb, request.EventScheduleId, cancellationToken);
                                }
                            }
                        }
                        return Result.Ok(request);
                    }
                    return Result.Fail<Contract>("Agendamento não existe");
                }
                return Result.Fail<Contract>("Evento não existe");
            }

            private async Task<bool> SendEmailToUser(ObjectId userId, Event eventDb, string scheduleId, CancellationToken cancellationToken)
            {
                var userQry = await _db.UserCollection.FindAsync(u => u.Id == userId, cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                try
                {
                    if (user != null)
                    {
                        var emailData = new EmailUserData
                        {
                            Email = user.Email,
                            Name = user.Name,
                            ExtraData = new Dictionary<string, string>
                            {
                                { "name", user.Name },
                                { "eventTitle", eventDb.Title },
                                { "valuateUrl", _options.Value.SiteUrl + $"/configuracoes/avaliar-evento/{eventDb.Id}/{scheduleId}/" }
                            }
                        };
                        await _provider.SendEmail(emailData, "Avaliação de Evento", "BTG-EventStudentValuation");
                        await SaveNotification(user.Id, eventDb, scheduleId, true);
                        return true;
                    }
                    // TODO: Erro usuario não existe na base
                    return false;
                }
                catch (Exception ex)
                {
                    await SaveNotification(user.Id, eventDb, scheduleId, false);
                    return false;
                }
            }

            private async Task<bool> SaveNotification(ObjectId userId, Event dbEvent, string scheduleId, bool emailDelivered)
            {
                var path = "/configuracoes/avaliar-evento/" + dbEvent.Id + "/" + scheduleId + "/" + dbEvent.Title + "/" + dbEvent.CreatedAt.ToString("yyyy-MM-dd");

                var notification = Notification.Create(
                    userId, emailDelivered,
                    "Avalie o Evento que participou",
                    $"O formulário de avaliação da aula presencial {dbEvent.Title} já está disponível. Nos conte o que você achou.",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
