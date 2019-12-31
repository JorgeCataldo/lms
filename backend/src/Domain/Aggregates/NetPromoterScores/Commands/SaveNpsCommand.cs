using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.NetPromoterScores.Commands
{
    public class SaveNpsCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public decimal Grade { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userId = ObjectId.Parse(request.UserId);
                var user = await GetUser(userId, cancellationToken);
                if(user == null)
                    return Result.Fail<Contract>("Usuário não encontrado");

                var npsBase = await _db.NetPromoterScoresCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId)
                    .FirstOrDefaultAsync();

                if (npsBase != null)
                    return Result.Fail<Contract>("Você ja avaliou o sistema");

                var nps = NetPromoterScore.Create(userId, request.Grade, user.Name, user.Email, 
                    user.TracksInfo, user.ModulesInfo, user.EventsInfo);

                if (nps.IsFailure)
                    return Result.Fail<Contract>("Ocorreu um erro na criação da avaliação");

                await _db.NetPromoterScoresCollection.InsertOneAsync(nps.Data);
                
                return Result.Ok(request);
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                return await _db.UserCollection
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == userId, token);
            }
        }
    }
}
