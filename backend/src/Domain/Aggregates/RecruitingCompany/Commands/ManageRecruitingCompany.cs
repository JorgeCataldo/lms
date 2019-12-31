using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.RecruitingCompany.RecruitingCompany;

namespace Domain.Aggregates.RecruitingCompany.Commands
{
    public class ManageRecruitingCompany
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public RecruitingCompanyItem Company { get; set; }
        }

        public class RecruitingCompanyItem
        {
            public string SocialName { get; set; }
            public string BusinessName { get; set; }
            public string Cnpj { get; set; }
            public Address Address { get; set; }
            public CompanyContactItem HumanResourcesResponsible { get; set; }
            public CompanyContactItem OperationsResponsible { get; set; }
            public long? CompanySize { get; set; }
            public string BusinessActivity { get; set; }
            public long? YearlyHiring { get; set; }
            public string ProfileMeasuringTool { get; set; }
            public bool AuthLogo { get; set; }
            public string LogoUrl { get; set; }
        }

        public class CompanyContactItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Recruiter" && request.UserRole!= "BusinessManager") //verificar regra de negócio para o role BusinessManager
                    return Result.Fail<Result>("Acesso Negado");

                if (request.Company == null)
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var company = await _db.RecruitingCompanyCollection.AsQueryable()
                    .Where(x => x.CreatedBy == userId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                CompanyContact hrResponsible = null;
                CompanyContact opResponsible = null;

                if (request.Company.HumanResourcesResponsible != null)
                {
                    hrResponsible = new CompanyContact() {
                        Email = request.Company.HumanResourcesResponsible.Email,
                        Name = request.Company.HumanResourcesResponsible.Name,
                        Phone = request.Company.HumanResourcesResponsible.Phone,
                    };
                }

                if (request.Company.OperationsResponsible != null)
                {
                    opResponsible = new CompanyContact() {
                        Email = request.Company.OperationsResponsible.Email,
                        Name = request.Company.OperationsResponsible.Name,
                        Phone = request.Company.OperationsResponsible.Phone,
                    };
                }

                if (company == null)
                {
                    company = RecruitingCompany.Create(
                        userId, request.Company.SocialName, request.Company.BusinessName, request.Company.Cnpj,
                        request.Company.Address, hrResponsible, opResponsible,
                        request.Company.CompanySize, request.Company.BusinessActivity,
                        request.Company.YearlyHiring, request.Company.ProfileMeasuringTool,
                        request.Company.AuthLogo, request.Company.LogoUrl
                    ).Data;

                    await _db.RecruitingCompanyCollection.InsertOneAsync(
                        company, cancellationToken: cancellationToken
                    );

                    var user = await _db.UserCollection.AsQueryable()
                        .Where(u => u.Id == userId)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    user.FirstAccess = false;
                    await _db.UserCollection.ReplaceOneAsync(
                        u => u.Id == user.Id, user,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    company.SocialName = request.Company.SocialName;
                    company.BusinessName = request.Company.BusinessName;
                    company.Cnpj = request.Company.Cnpj;
                    company.Address = request.Company.Address;
                    company.HumanResourcesResponsible = hrResponsible;
                    company.OperationsResponsible = opResponsible;
                    company.CompanySize = request.Company.CompanySize;
                    company.BusinessActivity = request.Company.BusinessActivity;
                    company.YearlyHiring = request.Company.YearlyHiring;
                    company.ProfileMeasuringTool = request.Company.ProfileMeasuringTool;
                    company.AuthLogo = request.Company.AuthLogo;
                    company.LogoUrl = request.Company.LogoUrl;

                    await _db.RecruitingCompanyCollection.ReplaceOneAsync(
                        t => t.CreatedBy == userId, company,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }
        }
    }
}
