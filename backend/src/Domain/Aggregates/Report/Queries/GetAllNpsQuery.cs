using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Report.Queries
{
    public class GetAllNpsQuery
    {
        public class Contract : IRequest<Result<List<NpsItemToExport>>>
        {
            public string UserRole { get; set; }
        }

        public class NpsItem
        {
            public ObjectId UserId { get; set; }
            public string Name { get; set; }
            public string Cpf { get; set; }
            public string Email { get; set; }
            public decimal Grade { get; set; }
            public DateTimeOffset Date { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> EventsInfo { get; set; }
        }

        public class NpsItemToExport
        {
            public string Name { get; set; }
            public string Cpf { get; set; }
            public string Email { get; set; }
            public decimal Grade { get; set; }
            public string Date { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> EventsInfo { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<NpsItemToExport>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<NpsItemToExport>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.UserRole != "Admin" && request.UserRole != "Secretary")
                        return Result.Fail<List<NpsItemToExport>>("Acesso Negado");

                    var users = await _db.UserCollection
                        .AsQueryable()
                        .ToListAsync();

                    var nps = await _db.NetPromoterScoresCollection
                        .AsQueryable()
                        .Select(x => new NpsItem
                        {
                            UserId = x.UserId,
                            Name = x.Name,
                            Email = x.Email,
                            Grade = x.Grade,
                            Date = x.CreatedAt,
                            TracksInfo = x.TracksInfo,
                            ModulesInfo = x.ModulesInfo,
                            EventsInfo = x.EventsInfo
                        })
                        .ToListAsync();
                    
                    var npsToExport = nps.Select(x => new NpsItemToExport
                    {
                        Name = x.Name,
                        Cpf = users.Where(u => u.Id == x.UserId).Select(u => u.Cpf).First(),
                        Email = x.Email,
                        Grade = x.Grade,
                        Date = x.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                        TracksInfo = x.TracksInfo,
                        ModulesInfo = x.ModulesInfo,
                        EventsInfo = x.EventsInfo
                    }).ToList();


                    return Result.Ok(npsToExport);
                }
                catch(Exception ex)
                {
                    return Result.Fail<List<NpsItemToExport>>("Acesso Negado");
                }
            }
        }
    }
}
