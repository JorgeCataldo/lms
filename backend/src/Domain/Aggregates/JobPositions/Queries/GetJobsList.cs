using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.JobPosition;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.RecruitingCompany.Queries
{
    public class GetJobsList
    {
        public class Contract : CommandContract<Result<PagedJobsItems>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public RequestFilters Filters { get; set; }
            public string UserRole { get; set; }
            public ObjectId UserId { get; set; }
        }

        public class RequestFilters
        {
            public string Name { get; set; }
            public List<UserCategoryFilter> CategoryFilter { get; set; }
        }

        public class UserCategoryFilter
        {
            public string ColumnName { get; set; }
            public List<string> ContentNames { get; set; }
        }

        public class PagedJobsItems
        {
            public int Page { get; set; }
            public long ItemsCount { get; set; }
            public List<JobItem> JobItems { get; set; }
        }

        public class JobItem
        {
            public ObjectId JobPositionId { get; set; }
            public string JobTitle { get; set; }
            public string RecruitingCompanyName { get; set; }
            public string RecruitingCompanyLogoUrl { get; set; }
            public Employment Employment { get; set; }
            public bool? UserApplicationApproved { get; set; }
            public ObjectId? UserApplicationCreatedBy { get; set; }
            public bool? UserApplicationAccepted { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedJobsItems>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedJobsItems>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "Admin")
                    return Result.Fail<PagedJobsItems>("Acesso Negado");

                var jobItems = new List<JobItem>();

                var queryable = _db.JobPositionCollection.AsQueryable();

                queryable = SetFilters(request, queryable);

                var jobs = await queryable.ToListAsync();
                
                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    request.Filters.CategoryFilter = request.Filters.CategoryFilter.Where(x => x.ContentNames.Count > 0).ToList();
                    
                    MixCompFormFilter(request.Filters.CategoryFilter);
                    MixLanguagesFilter(request.Filters.CategoryFilter);

                    foreach (UserCategoryFilter userCategoryFilter in request.Filters.CategoryFilter)
                    {
                        foreach (string contentName in userCategoryFilter.ContentNames)
                        {
                            jobs = FilterByCareer(
                                jobs, userCategoryFilter.ColumnName, contentName
                            );
                        }
                    }
                }

                var jobUsersIds = jobs.Select(x => x.CreatedBy).ToList();

                var recruitingCompanies = await _db.RecruitingCompanyCollection.AsQueryable()
                    .Where(r => jobUsersIds.Contains(r.CreatedBy))
                    .ToListAsync();

                var userApplications = await _db.CandidateCollection.AsQueryable()
                   .Where(c => c.UserId == request.UserId)
                   .ToListAsync();

                foreach (JobPosition.JobPosition job in jobs)
                {
                    var recruitingCompany = recruitingCompanies.FirstOrDefault(x => x.CreatedBy == job.CreatedBy);
                    var jobItem  = new JobItem
                    {
                        JobPositionId = job.Id,
                        JobTitle = job.Title,
                        RecruitingCompanyName = recruitingCompany != null ? recruitingCompany.BusinessName : "",
                        RecruitingCompanyLogoUrl = recruitingCompany != null ? recruitingCompany.LogoUrl : "",
                        Employment = job.Employment
                    };
                    var isCadidate = userApplications.FirstOrDefault(x => x.JobPositionId == job.Id);

                    if (isCadidate != null)
                    {
                        jobItem.UserApplicationApproved = isCadidate.Approved;
                        jobItem.UserApplicationCreatedBy = isCadidate.CreatedBy;
                        jobItem.UserApplicationAccepted = isCadidate.Accepted;
                    }

                    jobItems.Add(jobItem);
                }

                if (request.Filters.CategoryFilter != null && request.Filters.CategoryFilter.Count > 0)
                {
                    var candidateFilter = request.Filters.CategoryFilter.Where(x => x.ColumnName == "candidate.type").FirstOrDefault();
                    if (candidateFilter != null && candidateFilter.ContentNames.Count > 0)
                    {
                        for (int i = 0; i < candidateFilter.ContentNames.Count; i++)
                        {
                            var contentName = candidateFilter.ContentNames[i];
                            switch(contentName)
                            {
                                case "Candidate":
                                    jobItems = jobItems.Where(x => x.UserApplicationApproved != null).ToList();
                                    break;
                                case "Pending":
                                    jobItems = jobItems.Where(x => x.UserApplicationCreatedBy != null && 
                                        x.UserApplicationCreatedBy != request.UserId &&
                                        x.UserApplicationAccepted == null).ToList();
                                    break;
                            }
                        }
                    }
                }

                var result = new PagedJobsItems()
                {
                    Page = request.Page,
                    ItemsCount = jobItems.Count,
                    JobItems = jobItems
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList()
                };

                return Result.Ok(result);
            }

            private IMongoQueryable<JobPosition.JobPosition> SetFilters(Contract request, IMongoQueryable<JobPosition.JobPosition> queryable)
            {
                queryable = queryable.Where(u =>
                    u.Status == JobPosition.JobPositionStatusEnum.Open
                );

                if (request.Filters == null) return queryable;

                if (!String.IsNullOrEmpty(request.Filters.Name))
                {
                    queryable = queryable.Where(job =>
                        job.Title.ToLower()
                            .Contains(request.Filters.Name.ToLower())
                        );
                }

                return queryable;
            }

            private List<JobPosition.JobPosition> FilterByCareer(
                List<JobPosition.JobPosition> jobs, string columnName, string value
            )
            {
                if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(value))
                    return jobs;

                switch (columnName)
                {
                    case "hist.degree":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.Education == value
                        ).ToList();
                    case "hist.collegeName":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.CurseName.ToLower().Contains(value.ToLower())
                        ).ToList();
                    case "hist.year":
                        var yearSemester = value.Split('.');
                        var year = yearSemester[0]; var semester = yearSemester[1];
                        var start = 1; var end = 6;
                        if (semester == "2") { start = 6; end = 12; }
                        var yearSemesterList = new List<string>();
                        for(int i = start; i <= end; i++)
                        {
                            var startString = "0";
                            if (i > 9) { startString = i.ToString(); }
                            else { startString = startString + i.ToString(); }
                            yearSemesterList.Add(startString + year);
                        }
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            yearSemesterList.Contains(u.Employment.PreRequirements.DateConclusion)
                        ).ToList();
                    case "hist.cr":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            !string.IsNullOrEmpty(u.Employment.PreRequirements.CrAcumulation) &&
                            u.Employment.PreRequirements.CrAcumulation.All(x => char.IsDigit(x) || x == '.' || x == ',') &&
                            decimal.Parse(u.Employment.PreRequirements.CrAcumulation.Replace(',', '.'), CultureInfo.InvariantCulture) >= decimal.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture)
                        ).ToList();
                    case "compForm":
                        var nameValue = value.Split('-');
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.ComplementaryInfo.Any(t => 
                                t.Name.ToLower().Contains(nameValue[0].ToLower()) &&
                                t.Level.ToLower().Contains(nameValue[1].ToLower())
                            )
                        ).ToList();
                    case "compForm.name":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.ComplementaryInfo.Any(t => t.Name.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compForm.level":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.ComplementaryInfo.Any(t => t.Level.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.certificates":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.Certification.ToLower().Contains(value.ToLower())
                        ).ToList();
                    case "compInfo":
                        var languageValue = value.Split('-');
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.LanguageInfo.Any(t =>
                                t.Language.ToLower().Contains(languageValue[0].ToLower()) &&
                                t.Level.ToLower().Contains(languageValue[1].ToLower())
                            )
                        ).ToList();
                    case "compInfo.languages":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.LanguageInfo.Any(t => t.Language.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    case "compInfo.languages.level":
                        return jobs.Where(u =>
                            u.Employment != null &&
                            u.Employment.PreRequirements != null &&
                            u.Employment.PreRequirements.LanguageInfo.Any(t => t.Level.ToLower().Contains(value.ToLower()))
                        ).ToList();
                    default:
                        return jobs;
                }
            }

            private void MixCompFormFilter(List<UserCategoryFilter> CategoryFilters)
            {
                var compFormName = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compForm.name");
                var compFormLevel = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compForm.level");
                if (compFormName != null && compFormLevel != null)
                {
                    var mixedUserCategoryFilter = new UserCategoryFilter
                    {
                        ColumnName  = "compForm",
                        ContentNames = new List<string>()
                    };

                    for(int i = 0; i < compFormName.ContentNames.Count; i++)
                    {
                        for (int j = 0; j < compFormLevel.ContentNames.Count; j++)
                        {
                            mixedUserCategoryFilter.ContentNames.Add(compFormName.ContentNames[i] + "-" + compFormLevel.ContentNames[j]);
                        }
                    };

                    CategoryFilters.Remove(compFormName);
                    CategoryFilters.Remove(compFormLevel);
                    CategoryFilters.Add(mixedUserCategoryFilter);
                }
            }

            private void MixLanguagesFilter(List<UserCategoryFilter> CategoryFilters)
            {
                var compFormName = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compInfo.languages");
                var compFormLevel = CategoryFilters.FirstOrDefault(x => x.ColumnName == "compInfo.languages.level");
                if (compFormName != null && compFormLevel != null)
                {
                    var mixedUserCategoryFilter = new UserCategoryFilter
                    {
                        ColumnName = "compInfo",
                        ContentNames = new List<string>()
                    };

                    for (int i = 0; i < compFormName.ContentNames.Count; i++)
                    {
                        for (int j = 0; j < compFormLevel.ContentNames.Count; j++)
                        {
                            mixedUserCategoryFilter.ContentNames.Add(compFormName.ContentNames[i] + "-" + compFormLevel.ContentNames[j]);
                        }
                    };

                    CategoryFilters.Remove(compFormName);
                    CategoryFilters.Remove(compFormLevel);
                    CategoryFilters.Add(mixedUserCategoryFilter);
                }
            }
        }
    }
}
