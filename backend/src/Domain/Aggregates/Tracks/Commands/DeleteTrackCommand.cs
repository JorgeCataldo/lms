using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Commands
{
    public class DeleteTrackCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string TrackId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var tId = ObjectId.Parse(request.TrackId);
                var userId = ObjectId.Parse(request.UserId);

                var track = await (await _db
                        .Database
                        .GetCollection<Track>("Tracks")
                        .FindAsync(x => x.Id == tId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                
                if (track == null)
                    return Result.Fail<bool>("Trilha não Encontrada");

                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail<bool>("Acesso Negado");

                if(track.CreatedBy != userId)
                    return Result.Fail<bool>("Você não tem permissão de excluir a trilha selecionada.");

                track.DeletedBy = ObjectId.Parse(request.UserId);
                track.DeletedAt = DateTimeOffset.Now;

                await _db.TrackCollection.ReplaceOneAsync(t => t.Id == track.Id, track, cancellationToken: cancellationToken);
                
                return Result.Ok(true);

            }
        }
    }
}
