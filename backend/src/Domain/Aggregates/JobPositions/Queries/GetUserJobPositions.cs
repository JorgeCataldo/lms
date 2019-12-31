using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
namespace Domain.Aggregates.JobPosition.Queries
{
    public class GetUserJobPositions
    {
        public class Contract : CommandContract<Result<ApplicationItem>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class ApplicationItem
        {
            public int TotalOpenJobs { get; set; }
            public List<UserApplicationItem> UserApplications { get; set; }
        }

        public class UserApplicationItem
        {
            public ObjectId JobPositionId { get; set; }
            public string JobTitle { get; set; }
            public string RecruitingCompanyName { get; set; }
            public DateTimeOffset DueTo { get; set; }
            public bool? Approved { get; set; }
            public ObjectId CreatedBy { get; set; }
            public bool Accepted { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ApplicationItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<ApplicationItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "Admin")
                    return Result.Fail<ApplicationItem>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                ApplicationItem application = new ApplicationItem
                {
                    TotalOpenJobs = 0,
                    UserApplications = new List<UserApplicationItem>()
                };

                var userApplications = await _db.CandidateCollection.AsQueryable()
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                application.TotalOpenJobs = await _db.JobPositionCollection.AsQueryable()
                    .Where(j => j.Status == JobPositionStatusEnum.Open)
                    .CountAsync();

                if (userApplications == null || userApplications.Count == 0)
                    return Result.Ok(application);

                var jobsIds = userApplications.Select(x => x.JobPositionId).ToList();

                var jobs = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => 
                        jobsIds.Contains(p.Id) &&
                        p.Status == JobPositionStatusEnum.Open
                    )
                    .ToListAsync();

                var jobUsersIds = jobs.Select(x => x.CreatedBy).ToList();

                var recruitingCompanies = await _db.RecruitingCompanyCollection.AsQueryable()
                    .Where(r => jobUsersIds.Contains(r.CreatedBy))
                    .ToListAsync();

                foreach (Candidate.Candidate userApplication in userApplications)
                {
                    var job = jobs.FirstOrDefault(x => x.Id == userApplication.JobPositionId);
                    if (job != null)
                    {
                        var recruitingCompany = recruitingCompanies.FirstOrDefault(x => x.CreatedBy == job.CreatedBy);
                        if (recruitingCompany != null)
                        {
                            application.UserApplications.Add(new UserApplicationItem
                            {
                                JobPositionId = userApplication.JobPositionId,
                                JobTitle = job.Title,
                                RecruitingCompanyName = recruitingCompany.BusinessName,
                                DueTo = job.DueTo,
                                Approved = userApplication.Approved,
                                CreatedBy = userApplication.CreatedBy,
                                Accepted = userApplication.Accepted
                            });
                        }
                    }
                }

                return Result.Ok(application);
            }
        }
    }
}
