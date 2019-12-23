using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Notifications.Commands
{
    public class ManageNotificationCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string UserId { get; set; }
            public string NotificationId { get; set; }
            public bool Read { get; set; }
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
                    if (String.IsNullOrEmpty(request.NotificationId))
                        return Result.Fail("Id da notificação não informado");

                    var notification = await GetNotification(request.NotificationId, cancellationToken);
                    if (notification == null)
                        return Result.Fail("Pergunta não existe");
                    if (notification.UserId != ObjectId.Parse(request.UserId))
                        return Result.Fail("Acesso negado");

                    notification.Read = request.Read;

                    await _db.NotificationCollection.ReplaceOneAsync(n =>
                        n.Id == notification.Id, notification,
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

            private async Task<Notification> GetNotification(string rNotificationId, CancellationToken cancellationToken)
            {
                var notificationId = ObjectId.Parse(rNotificationId);

                var query = await _db.Database
                    .GetCollection<Notification>("Notifications")
                    .FindAsync(
                        x => x.Id == notificationId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }
        }
    }
}
