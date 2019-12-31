using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.NetPromoterScores.Queries
{
    public class GetAllNpsQuery
    {
        public class Contract : CommandContract<Result<List<NpsItem>>>
        {
            public string UserRole { get; set; }
        }

        public class NpsItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public decimal Grade { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> EventsInfo { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<NpsItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<NpsItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "Secretary")
                    return Result.Fail<List<NpsItem>> ("Acesso Negado");

                var nps = await _db.NetPromoterScoresCollection
                    .AsQueryable()
                    .Select(x => new NpsItem
                    {
                        Name = x.Name,
                        Email = x.Email,
                        Grade = x.Grade,
                        CreatedAt = x.CreatedAt,
                        TracksInfo = x.TracksInfo,
                        ModulesInfo = x.ModulesInfo,
                        EventsInfo = x.EventsInfo
                    })
                    .ToListAsync();

                return Result.Ok(nps);
            }
        }
    }
}
