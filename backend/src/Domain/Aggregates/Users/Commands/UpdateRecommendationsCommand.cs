using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class UpdateRecommendationsCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string CurrentUserRole { get; set; }
            public List<RecommendationItem> Recommendations { get; set; }
        }

        public class RecommendationItem
        {
            public string UserId { get; set; }
            public List<TrackItem> Tracks { get; set; }
            public List<ModuleItem> Modules { get; set; }
            public List<EventItem> Events { get; set; }
        }

        public class TrackItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int? ValidFor { get; set; }
            public DateTimeOffset DueDate { get; set; }
        }

        public class ModuleItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int? ValidFor { get; set; }
            public DateTimeOffset DueDate { get; set; }
        }

        public class EventItem
        {
            public string ScheduleId { get; set; }
            public string EventId { get; set; }
            public string Name { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db,
                UserManager<User> userManager, 
                IMediator mediator)
            {
                _db = db;
                _userManager = userManager;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {

                if (request.CurrentUserRole != "Admin" &&
                    request.CurrentUserRole != "HumanResources" &&
                    request.CurrentUserRole != "Secretary" && request.CurrentUserRole != "Recruiter")
                {
                    return Result.Fail<Contract>("Acesso Negado");
                }

                foreach (RecommendationItem recommendation in request.Recommendations)
                {
                    if (String.IsNullOrEmpty(recommendation.UserId))
                        return Result.Fail<Contract>("Usuário Inválido");

                    var userId = ObjectId.Parse(recommendation.UserId);

                    var user = await GetUserById(userId, cancellationToken);
                    if (user == null)
                        return Result.Fail<Contract>("Usuário não existe");

                    if (user.TracksInfo == null)
                        user.TracksInfo = new List<User.UserProgress>();
                    if (user.ModulesInfo == null)
                        user.ModulesInfo = new List<User.UserProgress>();
                    if (user.EventsInfo == null)
                        user.EventsInfo = new List<User.UserProgress>();

                    foreach (TrackItem rTrack in recommendation.Tracks)
                    {
                        var trackId = ObjectId.Parse(rTrack.Id);

                        var track = await GetTrackById(trackId, cancellationToken);
                        if (track == null)
                            return Result.Fail<Contract>("Trilha não existe");

                        var find = user.TracksInfo.FirstOrDefault(x => x.Id == trackId);
                        if (find == null)
                            user.TracksInfo = await AddTrackToUser(user, track, rTrack, cancellationToken);
                    }

                    foreach (ModuleItem rModule in recommendation.Modules)
                    {
                        var moduleId = ObjectId.Parse(rModule.Id);

                        var dbModule = await GetModuleById(moduleId, cancellationToken);
                        if (dbModule == null)
                            return Result.Fail<Contract>("Módulo não existe");

                        var find = user.ModulesInfo.FirstOrDefault(x => x.Id == moduleId);
                        if (find == null)
                        {
                            var userProgress = User.UserProgress.Create(
                                dbModule.Id, 0, 0, dbModule.Title, dbModule.ValidFor
                            ).Data;

                            user.ModulesInfo.Add(userProgress);
                        }

                        user = AddRequirements(user, dbModule.Requirements);
                    }

                    foreach (EventItem rEvent in recommendation.Events)
                    {
                        var eventId = ObjectId.Parse(rEvent.EventId);
                        var scheduleId = ObjectId.Parse(rEvent.ScheduleId);

                        var dbEvent = await GetEventById(eventId, cancellationToken);
                        if (dbEvent == null)
                            return Result.Fail<Contract>("Evento não existe");

                        if (dbEvent.Schedules == null)
                            return Result.Fail<Contract>("Data de Evento não existe");

                        var schedule = dbEvent.Schedules.FirstOrDefault(sc => sc.Id == scheduleId);
                        if (schedule == null)
                            return Result.Fail<Contract>("Data de Evento não existe");

                        var find = user.EventsInfo.FirstOrDefault(x => x.Id == scheduleId);
                        if (find == null)
                        {
                            var userProgress = User.UserProgress.Create(
                                scheduleId, 0, 0, dbEvent.Title, null
                            ).Data;

                            user.EventsInfo.Add(userProgress);
                        }

                        user = AddRequirements(user, dbEvent.Requirements);
                    }

                    await _db.UserCollection.ReplaceOneAsync(
                        t => t.Id == user.Id, user,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(request);
            }

            private User AddRequirements(User user, List<Requirement> requirements)
            {
                foreach (var reqModule in requirements)
                {
                    var findReq = user.ModulesInfo.FirstOrDefault(x => x.Id == reqModule.ModuleId);
                    if (findReq == null)
                    {
                        var userProgress = User.UserProgress.Create(
                            reqModule.ModuleId, 0, 0, "Requirement", null
                        ).Data;

                        user.ModulesInfo.Add(userProgress);
                    }
                }

                return user;
            }

            private async Task<User> GetUserById(ObjectId userId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(
                        x => x.Id == userId,
                        cancellationToken: token
                     );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<Track> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Track>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<Module> GetModuleById(ObjectId moduleId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == moduleId, cancellationToken: token);

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<Event> GetEventById(ObjectId eventId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.Id == eventId, cancellationToken: token);

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<List<User.UserProgress>> AddTrackToUser(
                User user, Track track, TrackItem rTrack, CancellationToken token
            ) {
                var userProgress = User.UserProgress.Create(
                    track.Id, rTrack.Level,
                    rTrack.Percentage, rTrack.Name, track.ValidFor
                ).Data;
                user.TracksInfo.Add(userProgress);

                track.StudentsCount++;
                await _db.TrackCollection.ReplaceOneAsync(
                    t => t.Id == track.Id, track,
                    cancellationToken: token
                );

                return user.TracksInfo;
            }
        }
    }
}
