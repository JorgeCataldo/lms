using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Notifications.Queries
{
    public class GetUserJobNotificationsQuery
    {
        public class Contract : CommandContract<Result<List<NotificationItem>>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class NotificationItem
        {
            public ObjectId NotificationId { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public bool Read { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<NotificationItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<NotificationItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student")
                    return Result.Ok(new List<NotificationItem>());

                var userId = ObjectId.Parse(request.UserId);

                var notifications = await _db.NotificationCollection
                    .AsQueryable()
                    .Where(x =>
                        x.UserId == userId &&
                        x.RedirectPath == "/minha-candidatura"
                    )
                    .Select(x => new NotificationItem
                    {
                        NotificationId = x.Id,
                        Title = x.Title,
                        Text = x.Text,
                        CreatedAt = x.CreatedAt,
                        Read = x.Read
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(cancellationToken);

                return Result.Ok(notifications);
            }
        }
    }
}
