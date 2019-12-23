using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.UsersCareer;
using static Domain.Aggregates.Users.User;
using System;
//using Domain.ValueObjects;

namespace Domain.Aggregates.Report.Queries
{
    public class ExportCareerQuery
    {
        public class Contract : IRequest<Result<List<UserItem>>>
        {
            public List<UsersToExport> Users { get; set; }
            public string UserRole { get; set; }
        }

        public class UsersToExport
        {
            public string Name { get; set; }
            public string UserId { get; set; }
        }

        public class UserItem
        {
            public string Name { get; set; }
            public Address Address { get; set; }
            public string BusinessGroup { get; set; }
            public string BusinessUnit { get; set; }
            public string Segment { get; set; }
            public UserCareer Career { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string State { get; set; }
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
                if (request.UserRole == "Student")
                    return Result.Fail<List<UserItem>>("Acesso Negado");

                var users = await GetUsersCareer(request, cancellationToken);

                return Result.Ok(users);
            }

            private async Task<List<UserItem>> GetUsersCareer(Contract request, CancellationToken token)
            {

                var careers = await _db.UserCareerCollection
                    .AsQueryable()
                    .ToListAsync();

                var userId = careers.Select(x => x.CreatedBy).ToList();

                var users = await _db.UserCollection
                    .AsQueryable()
                    .Where(x => userId.Contains(x.Id))
                    .ToListAsync();

                return careers.Select(career => new UserItem
                {
                    Name = users.First(u =>
                        u.Id == career.CreatedBy
                    ).Name,
                    Career = career,
                    Address = users.Where(u => u.Id == career.CreatedBy).Select(u => new Address
                    {
                        City = u.Address != null ? u.Address.City : null,
                        State = u.Address != null ? u.Address.State : null
                    }).FirstOrDefault(),
                    BusinessGroup = users.Where(u => u.Id == career.CreatedBy).Select(u => u.BusinessGroup != null ? u.BusinessGroup.Name : null).FirstOrDefault(),
                    BusinessUnit = users.Where(u => u.Id == career.CreatedBy).Select(u => u.BusinessUnit != null ? u.BusinessUnit.Name : null).FirstOrDefault(),
                    Segment = users.Where(u => u.Id == career.CreatedBy).Select(u => u.Segment != null ? u.Segment.Name : null).FirstOrDefault()
                }).ToList();

            }
        }
    }
}
