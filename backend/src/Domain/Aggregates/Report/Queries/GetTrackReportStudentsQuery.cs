using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.Modules;

namespace Domain.Aggregates.Report.Queries
{
    public class GetTrackReportStudentsQuery
    {
        public class Contract : IRequest<Result<List<StudentItem>>>
        {
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }


        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Cpf { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public bool IsBlocked { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string Responsible { get; set; }
            public RelationalItem Company { get; set; }
            public string RegistrationId { get; set; }
            public string UserName { get; set; }
            public Address Address { get; set; }
            public RelationalItem BusinessGroup { get; set; }
            public RelationalItem BusinessUnit { get; set; }
            public RelationalItem Segment { get; set; }
        }

        public class Address
        {
            public string State { get; set; }
            public string City { get; set; }
        }              

        public class Handler : IRequestHandler<Contract, Result<List<StudentItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<StudentItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Admin" || request.UserRole == "HumanResources")
                {
                    var students = await GetStudents(cancellationToken);

                    return Result.Ok(students);
                }

                else
                {
                    return Result.Fail<List<StudentItem>>("Acesso Negado");
                }
            }
            
            private async Task<List<StudentItem>> GetStudents(CancellationToken token)
            {
                var collection = _db.Database.GetCollection<StudentItem>("Users");

                var query = await collection.FindAsync(_ => true, cancellationToken: token);

                var users = await query.ToListAsync();

                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].ResponsibleId == null)
                        break;

                    users[i].Responsible = users.Where(x => x.Id == users[i].ResponsibleId).Select(x => x.Name).FirstOrDefault();
                }
                return users;
            }
        }
    }
}
