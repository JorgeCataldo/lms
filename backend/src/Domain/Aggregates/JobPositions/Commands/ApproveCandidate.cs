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
    public class ApproveCandidate
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserRole { get; set; }
            public string JobPositionId { get; set; }
            public string CurrentUserId { get; set; }
            public string UserId { get; set; }
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
                if (request.CurrentUserRole != "Recruiter" && request.CurrentUserRole != "HumanResources")
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                var currentUserId = ObjectId.Parse(request.CurrentUserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var candidate = await _db.CandidateCollection.AsQueryable()
                    .Where(c => c.UserId == userId && c.JobPositionId == positionId)
                    .FirstOrDefaultAsync();

                candidate.Approved = true;

                await _db.CandidateCollection.ReplaceOneAsync(c =>
                    c.UserId == userId && c.JobPositionId == positionId,
                    candidate, cancellationToken: cancellationToken
                );

                var recruitingCompany = await _db.RecruitingCompanyCollection.AsQueryable()
                    .FirstOrDefaultAsync(r => r.CreatedBy == currentUserId);

                if (recruitingCompany != null)
                    await SaveNotification(userId, recruitingCompany.BusinessName);

                return Result.Ok();
            }

            private async Task<bool> SaveNotification(ObjectId userId, string companyName)
            {
                var path = "/minha-candidatura";

                var notification = Notification.Create(
                    userId, false,
                    "Parabéns você foi selecionado pela " + companyName,
                    "A empresa entrará em contato com você.",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
