using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Notifications.Queries
{
    public class GetUserNotificationsQuery
    {
        public class Contract : CommandContract<Result<List<NotificationItem>>>
        {
            public int PageSize { get; set; } = 10;
            public string UserId { get; set; }
        }

        public class NotificationItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public string RedirectPath { get; set; }
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
                try
                {
                    var options = new FindOptions<NotificationItem>() {
                        Limit = request.PageSize,
                        Sort = Builders<NotificationItem>.Sort.Descending("createdAt")
                    };

                    var builder = Builders<NotificationItem>.Filter;
                    var filters = builder.Eq(
                        "userId", ObjectId.Parse(request.UserId)
                    );

                    var collection = _db.Database.GetCollection<NotificationItem>("Notifications");

                    var query = await collection.FindAsync(
                        filters, options: options,
                        cancellationToken: cancellationToken
                    );

                    var notifications = await query.ToListAsync(cancellationToken);

                    return Result.Ok(notifications);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<NotificationItem>>(err.Message);
                }
            }
        }
    }
}
