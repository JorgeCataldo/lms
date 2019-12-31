using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackOverviewStudentsQuery
    {
        public class Contract : CommandContract<Result<List<UserItem>>>
        {
            public string TrackId { get; set; }
            public string UserRole { get; set; }
        }

        public class UserItem
        {
            public string Name { get; set; }
            public string Cpf { get; set; }
            public string IsBlocked { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<UserItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<UserItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" &&
                    request.UserRole != "Secretary" && request.UserRole != "Recruiter")
                    return Result.Fail<List<UserItem>>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<List<UserItem>>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);

                var dbUsers = await _db.UserCollection
                    .AsQueryable()
                    .Where(
                        x => x.TracksInfo != null &&
                        x.TracksInfo.Any(y => y.Id == trackId) &&
                        x.IsBlocked == false
                    )
                    .Select(x => new UserItem
                    {
                        Name = x.Name,
                        Cpf = x.Cpf,
                        IsBlocked = x.IsBlocked ? "BLOQUEADO" : "LIBERADO"
                    })
                    .ToListAsync(cancellationToken: cancellationToken);

                return Result.Ok(dbUsers);
            }
        }
    }
}
