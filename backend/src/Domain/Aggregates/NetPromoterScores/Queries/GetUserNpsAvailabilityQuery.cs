using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Activations;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.NetPromoterScores.Queries
{
    public class GetUserNpsAvailabilityQuery
    {
        public class Contract : CommandContract<Result<Activation>>
        {
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Activation>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<Activation>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Admin" || request.UserRole == "Secretary")
                    return Result.Ok(Activation.Create().Data);

                var userId = ObjectId.Parse(request.UserId);

                var nps = await _db.NetPromoterScoresCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId)
                    .FirstOrDefaultAsync();

                if (nps != null)
                    return Result.Ok(Activation.Create().Data);

                var activationStatus = await _db.ActivationsCollection
                    .AsQueryable().
                    FirstOrDefaultAsync(x => x.Type == ActivationTypeEnum.Nps);

                if (activationStatus == null || !activationStatus.Active)
                    return Result.Ok(Activation.Create().Data);

                var trackProgress = await _db.UserTrackProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        x.UserId == userId &&
                        x.Progress >= activationStatus.Percentage
                    )
                    .FirstOrDefaultAsync();

                if (trackProgress != null)
                    return Result.Ok(activationStatus);

                var moduleProgress = await _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        x.UserId == userId &&
                        x.Progress >= activationStatus.Percentage
                    )
                    .FirstOrDefaultAsync();

                if (moduleProgress != null)
                    return Result.Ok(activationStatus);

                if (trackProgress == null && moduleProgress == null)
                    return Result.Ok(Activation.Create().Data);

                return Result.Ok(activationStatus);
            }
        }
    }
}
