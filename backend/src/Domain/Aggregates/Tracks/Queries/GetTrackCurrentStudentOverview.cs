using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackCurrentStudentOverviewQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string TrackId { get; set; }
            public string StudentId { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<WarningItem> Warnings { get; set; }
            public decimal Conclusion { get; set; }
            public string CertificateUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public bool MandatoryCourseVideo { get; set; }
            public string CourseVideoUrl { get; set; }
            public int? CourseVideoDuration { get; set; }
            public bool RequireUserCareer { get; set; }
            public bool BlockedByUserCareer { get; set; } = false;
            public int? AllowedPercentageWithoutCareerInfo { get; set; }
            public UserProgress TrackInfo { get; set; }
            public List<CalendarEventItem> CalendarEvents { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public decimal StudentPercentage { get; set; }
            public bool StudentFinished { get; set; }
            public int Order { get; set; }
            public bool Blocked { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public int Duration { get; set; }
            public string Title { get; set; }
            public bool StudentFinished { get; set; }
            public int Order { get; set; }
            public bool? ForceProblemStatement { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
        }

        public class WarningItem
        {
            public string Text { get; set; }
            public DateTimeOffset? DueTo { get; set; }
            public string RedirectTo { get; set; }
        }

        public class CalendarEventItem
        {
            public int? Duration { get; set; }
            public string Title { get; set; }
            public DateTimeOffset EventDate { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<TrackItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<TrackItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<TrackItem>("Id da Trilha não informado");
                if (String.IsNullOrEmpty(request.StudentId))
                    return Result.Fail<TrackItem>("Id do Aluno não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await GetTrackById(trackId, cancellationToken);

                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                var studentId = ObjectId.Parse(request.StudentId);

                track = await GetConfigurations(track, studentId, cancellationToken);
                track = await GetWarnings(track, studentId, cancellationToken);

                return Result.Ok(track);
            }

            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetConfigurations(TrackItem track, ObjectId studentId, CancellationToken token)
            {

                var qry = await _db.Database
                    .GetCollection<User>("Users")
                    .FindAsync(x => x.Id == studentId, cancellationToken: token);

                var user = await qry.FirstOrDefaultAsync(cancellationToken: token);
                track.TrackInfo = user.TracksInfo.FirstOrDefault(x => x.Id == track.Id);

                decimal conclusion = 0;
                track.Conclusion = 0;

                foreach (var config in track.ModulesConfiguration)
                {
                    try
                    {
                        var query = await _db.Database
                            .GetCollection<Module>("Modules")
                            .FindAsync(x =>
                                x.Id == config.ModuleId,
                                cancellationToken: token
                            );

                        var progressColl = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                        var progQuery = await progressColl.FindAsync(x =>
                            x.ModuleId == config.ModuleId &&
                            x.UserId == studentId
                        );

                        var progress = await progQuery.FirstOrDefaultAsync();

                        if (progress == null)
                        {
                            config.StudentPercentage = 0;
                            config.StudentFinished = false;

                        }
                        else
                        {
                            config.StudentPercentage = progress.Progress;
                            config.StudentFinished = progress.Level > config.Level;
                        }

                        conclusion = conclusion +
                            (config.StudentFinished ? 1 : config.StudentPercentage);
                    }
                    catch (Exception err)
                    {
                        throw new Exception($"Erro ao buscar o modulo {config.ModuleId}", err);
                    }
                }

                if (track.ModulesConfiguration.Count > 0)
                    track.Conclusion = conclusion / (decimal)track.ModulesConfiguration.Count;

                if (track.RequireUserCareer)
                {
                    var hasCareer = await _db.UserCareerCollection.AsQueryable()
                        .AnyAsync(c => c.CreatedBy == user.Id);

                    if (!hasCareer)
                        track.BlockedByUserCareer = track.Conclusion >= ((decimal)track.AllowedPercentageWithoutCareerInfo) / 100;
                }

                return track;
            }

            private async Task<TrackItem> GetWarnings(TrackItem track, ObjectId studentId, CancellationToken token)
            {
                track.Warnings = new List<WarningItem>();

                foreach (var trackEvent in track.EventsConfiguration)
                {
                    var dbEvent = await GetEventById(trackEvent.EventId, token);
                    var schedule = dbEvent.Schedules.FirstOrDefault(s => s.Id == trackEvent.EventScheduleId);
                    trackEvent.ForceProblemStatement = dbEvent.ForceProblemStatement;
                    var requirementsIds = dbEvent.Requirements.Select(x => x.ModuleId).ToList();

                    if(requirementsIds.Count > 0 && trackEvent.ForceProblemStatement.HasValue)
                    {
                        var hasApplication = await CheckEventApplication(
                                schedule.Id, studentId, token
                            );

                        for (int i = 0; i < track.ModulesConfiguration.Count; i++)
                        {
                            if (requirementsIds.Contains(track.ModulesConfiguration[i].ModuleId) && !hasApplication && trackEvent.ForceProblemStatement == true)
                            {
                                track.ModulesConfiguration[i].Blocked = true;
                            }
                            else
                            {
                                track.ModulesConfiguration[i].Blocked = false;
                            }
                        }
                    }

                    if (schedule != null)
                    {
                        trackEvent.EventDate = schedule.EventDate;
                        trackEvent.Duration = schedule.Duration;

                        if (schedule.FinishedAt.HasValue)
                        {
                            trackEvent.StudentFinished = true;

                            var hasReaction = await CheckHasEventReaction(
                                studentId, schedule.Id, token
                            );

                            if (!hasReaction)
                            {
                                track.Warnings.Add(new WarningItem
                                {
                                    Text = "Avalie o evento " + dbEvent.Title,
                                    RedirectTo = "configuracoes/avaliar-evento/" +
                                        dbEvent.Id + "/" +
                                        schedule.Id + "/" +
                                        dbEvent.Title + "/" +
                                        schedule.EventDate.ToString("yyyy-MM-dd")
                                });
                            }
                        }
                        else if (
                            schedule.EventDate >= DateTimeOffset.Now &&
                            (schedule.EventDate - DateTimeOffset.Now).TotalDays < 14
                        )
                        {
                            var hasApplication = await CheckEventApplication(
                                schedule.Id, studentId, token
                            );

                            if (!hasApplication)
                            {
                                track.Warnings.Add(new WarningItem
                                {
                                    Text = "Responda o questionário preparatório para o evento " + dbEvent.Title,
                                    RedirectTo = "evento/" + dbEvent.Id + "/" + schedule.Id
                                });
                            }
                        }
                    }
                }

                return track;
            }

            private async Task<Event> GetEventById(ObjectId eventId, CancellationToken token)
            {
                try
                {
                    var query = await _db.Database
                        .GetCollection<Event>("Events")
                        .FindAsync(x => x.Id == eventId, cancellationToken: token);

                    return await query.FirstOrDefaultAsync(cancellationToken: token);
                }
                catch (Exception err)
                {
                    throw new Exception($"Erro ao buscar o evento {eventId}", err);
                }
            }

            private async Task<bool> CheckEventApplication(ObjectId scheduleId, ObjectId studentId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<EventApplication>("EventApplications")
                    .FindAsync(x =>
                        x.UserId == studentId &&
                        x.ScheduleId == scheduleId,
                        cancellationToken: token
                    );

                return await query.AnyAsync(cancellationToken: token);
            }

            private async Task<bool> CheckHasEventReaction(ObjectId userId, ObjectId scheduleId, CancellationToken token)
            {
                var query = await _db.Database
                    .GetCollection<EventReaction>("EventReactions")
                    .FindAsync(x =>
                        x.CreatedBy == userId &&
                        x.EventScheduleId == scheduleId,
                        cancellationToken: token
                     );

                return await query.AnyAsync(cancellationToken: token);
            }
        }
    }
}
