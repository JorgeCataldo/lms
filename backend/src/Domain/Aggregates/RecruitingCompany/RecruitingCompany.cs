using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.RecruitingCompany
{
    public class RecruitingCompany : Entity
    {
        public string SocialName { get; set; }
        public string BusinessName { get; set; }
        public string Cnpj { get; set; }
        public Address Address { get; set; }
        public CompanyContact HumanResourcesResponsible { get; set; }
        public CompanyContact OperationsResponsible { get; set; }
        public long? CompanySize { get; set; }
        public string BusinessActivity { get; set; }
        public long? YearlyHiring { get; set; }
        public string ProfileMeasuringTool { get; set; }
        public bool AuthLogo { get; set; }
        public string LogoUrl { get; set; }

        public static Result<RecruitingCompany> Create(
            ObjectId userId, string socialName, string businessName, string cnpj,
            Address address, CompanyContact hrResponsible, CompanyContact opResponsible,
            long? companySize, string businessActivity, long? yearlyHiring, string profileMeasuringtool, 
            bool authLogo, string logoUrl
        ) {
            return Result.Ok(
                new RecruitingCompany() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    SocialName = socialName,
                    BusinessName = businessName,
                    Cnpj = cnpj,
                    Address = address,
                    HumanResourcesResponsible = hrResponsible,
                    OperationsResponsible = opResponsible,
                    CompanySize = companySize,
                    BusinessActivity = businessActivity,
                    YearlyHiring = yearlyHiring,
                    ProfileMeasuringTool = profileMeasuringtool,
                    AuthLogo = authLogo,
                    LogoUrl = logoUrl
                }
            );
        }

        public class CompanyContact
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
    }
}