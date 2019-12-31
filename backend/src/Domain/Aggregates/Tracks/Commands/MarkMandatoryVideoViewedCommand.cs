using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class MarkMandatoryVideoViewedCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string UserId { get; set; }
            public string TrackId { get; set; }
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
                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail("Id da Trilha não informado");

                ObjectId userId = ObjectId.Parse(request.UserId);
                ObjectId trackId = ObjectId.Parse(request.TrackId);

                User user = await GetUser(userId, cancellationToken);

                var userTrack = user.TracksInfo.FirstOrDefault(t => t.Id == trackId);
                if (userTrack == null)
                    return Result.Fail("Acesso Negado");

                userTrack.ViewedMandatoryVideo = true;

                await _db.UserCollection.ReplaceOneAsync(t =>
                    t.Id == userId, user,
                    cancellationToken: cancellationToken
                );

                return Result.Ok(request);
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(x =>
                        x.Id == userId,
                        cancellationToken: token
                    );

                return await query.FirstOrDefaultAsync(
                    cancellationToken: token
                );
            }
        }
    }
}
