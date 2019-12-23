using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.RecruitingCompany.Queries
{
    public class GetRecruitingCompany
    {
        public class Contract : CommandContract<Result<RecruitingCompany>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<RecruitingCompany>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<RecruitingCompany>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Recruiter" && request.UserRole != "BusinessManager" && request.UserRole != "Admin" && request.UserRole != "HumanResources")
                    return Result.Fail<RecruitingCompany>("Acesso Negado");

                var company = await GetRecruitingCompany(request, cancellationToken);

                if (company == null)
                    return Result.Fail<RecruitingCompany>("Sem Dados");

                return Result.Ok(company);
            }

            private async Task<RecruitingCompany> GetRecruitingCompany(Contract request, CancellationToken token)
            {
                var userId = ObjectId.Parse(request.UserId);

                return await _db.RecruitingCompanyCollection
                    .AsQueryable()
                    .Where(x => x.CreatedBy == userId)
                    .FirstOrDefaultAsync(cancellationToken: token);
            }
        }
    }
}
