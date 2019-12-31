using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Notifications;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
namespace Domain.Aggregates.JobPosition.Commands
{
    public class AcceptJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserId { get; set; }
            public string CurrentUserRole { get; set; }
            public string JobPositionId { get; set; }
            public string CandidateId { get; set; }
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
                if (request.CurrentUserRole != "Recruiter" && request.CurrentUserRole != "Student" && request.CurrentUserRole != "HumanResources")
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.CurrentUserId);
                var positionId = ObjectId.Parse(request.JobPositionId);
                var candidateId = ObjectId.Parse(request.CandidateId);

                var candidate = await _db.CandidateCollection.AsQueryable()
                    .FirstOrDefaultAsync(c =>
                        c.JobPositionId == positionId &&
                        c.UserId == candidateId
                    );

                if (candidate == null || candidate.CreatedBy == userId)
                    return Result.Fail<Result>("Acesso Negado");

                candidate.Accepted = true;

                await _db.CandidateCollection.ReplaceOneAsync(c =>
                    c.UserId == candidateId && c.JobPositionId == positionId,
                    candidate, cancellationToken: cancellationToken
                );

                if (request.CurrentUserRole == "Recruiter")
                {
                    var recruitingCompany = await _db.RecruitingCompanyCollection.AsQueryable()
                        .FirstOrDefaultAsync(r => r.CreatedBy == userId);

                    if (recruitingCompany != null)
                    {
                        var job = await _db.JobPositionCollection.AsQueryable()
                            .FirstOrDefaultAsync(j => j.Id == positionId);

                        if (job != null)
                        {
                            await SaveNotification(candidateId, recruitingCompany.BusinessName, job.Title);
                        }
                    }
                }

                return Result.Ok();
            }

            private async Task<bool> SaveNotification(ObjectId userId, string companyName, string jobName)
            {
                var path = "/minha-candidatura";

                var notification = Notification.Create(
                    userId, false,
                    companyName + " aceitou a sua candidatura.",
                    "Você agora é candidato á vaga '" + jobName + "'.",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
