using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.Aggregates.Notifications;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.JobPosition.Commands
{
    public class AddCandidateToJobPosition
    {
        public class Contract : CommandContract<Result>
        {
            public string CurrentUserRole { get; set; }
            public string CurrentUserId { get; set; }
            public List<UserItem> Candidates { get; set; }
            public string JobPositionId { get; set; }
        }

        public class UserItem
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
        }

        public class CandidateItem
        {
            public ObjectId UserId { get; set; }
            public string UserName { get; set; }
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
                if (request.CurrentUserRole != "Recruiter"  && request.CurrentUserRole != "HumanResources")
                    return Result.Fail("Acesso Negado");

                if (request.Candidates == null || request.Candidates.Count == 0)
                    return Result.Fail("Candidatos não informados");

                var recruiterId = ObjectId.Parse(request.CurrentUserId);
                var positionId = ObjectId.Parse(request.JobPositionId);

                var candidates = request.Candidates.Select(x => new CandidateItem
                {
                    UserId = ObjectId.Parse(x.UserId),
                    UserName = x.UserName
                });

                var position = await _db.JobPositionCollection.AsQueryable()
                    .Where(p => p.CreatedBy == recruiterId && p.Id == positionId)
                    .FirstOrDefaultAsync();

                if (position == null)
                    return Result.Fail("Vaga não encontrada");

                var candidatesIds = await _db.CandidateCollection.AsQueryable()
                   .Where(c => c.JobPositionId == positionId)
                   .Select(c => c.UserId)
                   .ToListAsync(cancellationToken);

                var candidatesToAdd = candidates.Where(x => !candidatesIds.Contains(x.UserId));

                var newCandidates = new List<Candidate.Candidate>();

                foreach (CandidateItem candidateToAdd in candidatesToAdd)
                {
                    var newCandidate = Candidate.Candidate.Create(
                        positionId, candidateToAdd.UserId, candidateToAdd.UserName, recruiterId
                    );

                    if (newCandidate.IsSuccess)
                    {
                        newCandidates.Add(newCandidate.Data);
                        await SaveNotification(candidateToAdd.UserId);
                    }
                }

                if (newCandidates.Count > 0)
                {
                    await _db.CandidateCollection.InsertManyAsync(
                        newCandidates, cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }

            private async Task<bool> SaveNotification(ObjectId userId)
            {
                var path = "/minha-candidatura";

                var notification = Notification.Create(
                    userId, false,
                    "Você foi selecionado para concorrer á uma vaga",
                    "Para participar do processo aprove o convite.",
                    path
                );

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
