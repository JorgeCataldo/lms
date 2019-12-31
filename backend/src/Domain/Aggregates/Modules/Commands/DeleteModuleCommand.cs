using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Tracks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class DeleteModuleCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string ModuleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "Author")
                    return Result.Fail("Acesso Negado");
                
                var mId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);

                var module = await _db.ModuleCollection.AsQueryable()
                    .Where(x => x.Id == mId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                
                if (module == null)
                    return Result.Fail("Módulo não Encontrado");
                
                if (module.CreatedBy != userId)
                    return Result.Fail<bool>("Você não tem permissão de excluir a trilha selecionada.");

                if (request.UserRole == "Student")
                {
                    if (!Module.IsInstructor(module, userId).Data)
                        return Result.Fail("Acesso Negado");
                }

                module.DeletedBy = userId;
                module.DeletedAt = DateTimeOffset.Now;

                await RemoveModuleFromTracks(module, cancellationToken);
                await RemoveModuleFromRequirements(module, cancellationToken);

                await _db.ModuleCollection.ReplaceOneAsync(t =>
                    t.Id == module.Id, module,
                    cancellationToken: cancellationToken
                );
                
                return Result.Ok();
            }

            private async Task<bool> RemoveModuleFromTracks(Module module, CancellationToken token)
            {
                var tracks = await _db.Database
                    .GetCollection<Track>("Tracks").AsQueryable()
                    .Where(t =>
                        t.ModulesConfiguration != null &&
                        t.ModulesConfiguration.Count > 0 && (
                            t.DeletedAt == null ||
                            t.DeletedAt == DateTimeOffset.MinValue
                        )
                    )
                    .ToListAsync();

                var tracksToUpdate = tracks.Where(t =>
                    t.ModulesConfiguration
                        .Select(m => m.ModuleId)
                        .Contains(module.Id)
                );

                foreach (Track track in tracksToUpdate)
                {
                    track.ModulesConfiguration = track.ModulesConfiguration.Where(m =>
                        m.ModuleId != module.Id
                    ).ToList();
                
                    await _db.TrackCollection.ReplaceOneAsync(t =>
                        t.Id == track.Id, track,
                        cancellationToken: token
                    );
                }

                return true;
            }

            private async Task<bool> RemoveModuleFromRequirements(Module module, CancellationToken token)
            {
                var modules = await _db.ModuleCollection.AsQueryable()
                    .Where(t =>
                        t.Requirements != null &&
                        t.Requirements.Count > 0 && (
                            t.DeletedAt == null ||
                            t.DeletedAt == DateTimeOffset.MinValue
                        )
                    )
                    .ToListAsync();

                foreach (Module dbModule in modules)
                {
                    dbModule.Requirements = dbModule.Requirements.Where(m =>
                        m.ModuleId != module.Id
                    ).ToList();

                    await _db.ModuleCollection.ReplaceOneAsync(t =>
                        t.Id == dbModule.Id, dbModule,
                        cancellationToken: token
                    );
                }

                var events = await _db.EventCollection.AsQueryable()
                    .Where(t =>
                        t.Requirements != null &&
                        t.Requirements.Count > 0 && (
                            t.DeletedAt == null ||
                            t.DeletedAt == DateTimeOffset.MinValue
                        )
                    )
                    .ToListAsync();

                foreach (Event dbEvent in events)
                {
                    dbEvent.Requirements = dbEvent.Requirements.Where(m =>
                        m.ModuleId != module.Id
                    ).ToList();

                    await _db.EventCollection.ReplaceOneAsync(t =>
                        t.Id == dbEvent.Id, dbEvent,
                        cancellationToken: token
                    );
                }

                return true;
            }
        }
    }
}
