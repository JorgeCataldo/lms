using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class BlockUserMaterial
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public string TrackId { get; set; }
            public string ModuleId { get; set; }
            public string EventScheduleId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>{
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student")
                    return Result.Fail("Acesso Negado");

                var userQry = await _db.UserCollection.FindAsync(u => u.Id == ObjectId.Parse(request.UserId), cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não existe");

                if (!string.IsNullOrEmpty(request.ModuleId))
                {
                    var moduleId = ObjectId.Parse(request.ModuleId);
                    user.ModulesInfo = user.ModulesInfo ?? new List<User.UserProgress>();
                    var moduleInfo = user.ModulesInfo.Find(x => x.Id == moduleId);
                    if (moduleInfo == null)
                    {
                        return Result.Fail("Usuário não esta associado a este módulo");
                    }
                    moduleInfo.Blocked = moduleInfo.Blocked.HasValue ? !moduleInfo.Blocked.Value : true;
                }
                else if (!string.IsNullOrEmpty(request.TrackId))
                {
                    var trackId = ObjectId.Parse(request.TrackId);
                    user.TracksInfo = user.TracksInfo ?? new List<User.UserProgress>();
                    var trackInfo = user.TracksInfo.Find(x => x.Id == trackId);
                    if (trackInfo == null)
                    {
                        return Result.Fail("Usuário não esta associado a esta trílha");
                    }
                    trackInfo.Blocked = trackInfo.Blocked.HasValue ? !trackInfo.Blocked.Value : true;
                }
                else if (!string.IsNullOrEmpty(request.EventScheduleId))
                {
                    var eventScheduleId = ObjectId.Parse(request.EventScheduleId);
                    user.EventsInfo = user.EventsInfo ?? new List<User.UserProgress>();
                    var eventInfo = user.EventsInfo.Find(x => x.Id == eventScheduleId);
                    if (eventInfo == null)
                    {
                        return Result.Fail("Usuário não esta associado a este evento");
                    }
                    eventInfo.Blocked = eventInfo.Blocked.HasValue ? !eventInfo.Blocked.Value : true;
                }
                await _db.UserCollection.ReplaceOneAsync(t => t.Id == user.Id, user,
                      cancellationToken: cancellationToken);

                return Result.Ok(request);
            }
        }
    }
}
