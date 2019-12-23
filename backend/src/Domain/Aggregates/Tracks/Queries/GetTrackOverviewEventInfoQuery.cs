using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using Domain.Aggregates.Responsibles;
using Microsoft.Extensions.Configuration;
using Domain.Aggregates.Events;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackOverviewEventInfoQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string TrackId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<LateStudentItem> LateStudents { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public List<ObjectId> IncompleteRequirementStudents { get; set; }
            public List<TrackEventApplicationItem> Applications { get; set; }
            public decimal Weight { get; set; }
            public List<string> Keys { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class TrackEventApplicationItem
        {
            public ObjectId UserId { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public decimal? OrganicGrade { get; set; }
            public decimal? InorganicGrade { get; set; }
            public List<CustomEventGradeValue> CustomEventGradeValues { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public decimal Weight { get; set; }
        }

        public class LateStudentItem
        {
            public ObjectId StudentId { get; set; }
            public decimal Progress { get; set; }
            public List<ObjectId> LateEvents { get; set; }
            public int EventsTotal { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
        }

        public class StudentsProgressItem
        {
            public int? Level { get; set; }
            public int Count { get; set; }
        }

        public class TrackItemWeight
        {
            public ObjectId ItemId { get; set; }
            public decimal Weight { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<TrackItem>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<TrackItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                string appEventGradeKeys = _configuration[$"Permissions:EventGradeKeys"];
                var eventGradeKeys = new List<string>();
                if (!string.IsNullOrEmpty(appEventGradeKeys))
                {
                    eventGradeKeys = appEventGradeKeys.Split(',').ToList();
                }
                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<TrackItem>("Id da Trilha não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await GetTrackById(trackId, cancellationToken);

                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                if (request.UserRole == "Admin" || request.UserRole == "HumanResources" || request.UserRole == "Secretary")
                {
                    track = await GetTrackStudents(track, request, eventGradeKeys, cancellationToken);

                    return Result.Ok(track);
                }
                else if (request.UserRole == "BusinessManager")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var responsible = await _db
                        .Database
                        .GetCollection<Responsible>("Responsibles")
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId);

                    if (responsible == null)
                        return Result.Fail<TrackItem>("Sem usuários associados");

                    var subordinatesOnTrack = await _db.UserCollection
                        .AsQueryable()
                        .Where(x =>
                            responsible.SubordinatesUsersIds.Contains(x.Id) &&
                            x.TracksInfo.Any(y => y.Id == track.Id && y.Blocked != true)
                        )
                        .ToListAsync(cancellationToken);

                    if (subordinatesOnTrack.Count == 0)
                        return Result.Fail<TrackItem>("Usários não associados à trilha");

                    track = await GetTrackStudents(track, request, eventGradeKeys, cancellationToken, responsible.SubordinatesUsersIds);
                    return Result.Ok(track);
                }
                else if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var responsible = await _db
                        .Database
                        .GetCollection<Responsible>("Responsibles")
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId);

                    var subordinatesOnTrack = new List<Users.User>();
                    if (responsible != null)
                    {
                        subordinatesOnTrack = await _db.UserCollection
                            .AsQueryable()
                            .Where(x =>
                                responsible.SubordinatesUsersIds.Contains(x.Id) &&
                                x.TracksInfo.Any(y => y.Id == track.Id && y.Blocked != true)
                            )
                            .ToListAsync(cancellationToken);
                    }

                    if (subordinatesOnTrack.Count == 0)
                    {
                        var InstructorEventsIds = await _db.EventCollection
                            .AsQueryable()
                            .Where(x => (x.InstructorId == userId || x.TutorsIds.Contains(userId)))
                            .Select(x => x.Id)
                            .ToListAsync(cancellationToken);

                        var InstructorModulesIds = await _db.ModuleCollection
                            .AsQueryable()
                            .Where(x => (x.InstructorId == userId || x.ExtraInstructorIds.Contains(userId) || x.TutorsIds.Contains(userId)))
                            .Select(x => x.Id)
                            .ToListAsync(cancellationToken);

                        var isInstructor =
                            track.ModulesConfiguration.Any(x => InstructorModulesIds.Contains(x.ModuleId)) ||
                            track.EventsConfiguration.Any(x => InstructorEventsIds.Contains(x.EventId));

                        if (!isInstructor)
                            return Result.Fail<TrackItem>("Acesso Negado");

                        track.EventsConfiguration = track.EventsConfiguration.Where(x => InstructorEventsIds.Contains(x.EventId)).ToList();
                        track = await GetTrackStudents(track, request, eventGradeKeys, cancellationToken);
                        return Result.Ok(track);
                    }
                    else
                    {
                        responsible.SubordinatesUsersIds.Add(userId);

                        track = await GetTrackStudents(track, request, eventGradeKeys, cancellationToken, responsible.SubordinatesUsersIds);
                        return Result.Ok(track);
                    }
                }
                else
                {
                    return Result.Fail<TrackItem>("Acesso Negado");
                }
            }

            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetTrackStudents(TrackItem track, Contract request, List<string> keys, CancellationToken token, List <ObjectId> subordinates = null)
            {
                if (track.EventsConfiguration == null)
                    return track;

                var trackItemsWeight = new List<TrackItemWeight>();

                trackItemsWeight.AddRange(track.EventsConfiguration
                    .Select(x => new TrackItemWeight
                    {
                        ItemId = x.EventId,
                        Weight = x.Weight
                    }).ToList());

                trackItemsWeight.AddRange(track.ModulesConfiguration
                    .Select(x => new TrackItemWeight
                    {
                        ItemId = x.ModuleId,
                        Weight = x.Weight
                    }).ToList());

                if (!trackItemsWeight.Any(x => x.Weight > 0))
                {
                    foreach (TrackItemWeight item in trackItemsWeight)
                    {
                        item.Weight = 100 / (decimal)trackItemsWeight.Count;
                    }

                    foreach (TrackEventItem trackEventItem in track.EventsConfiguration)
                    {
                        trackEventItem.Weight = trackItemsWeight.FirstOrDefault(x => x.ItemId == trackEventItem.EventId).Weight;
                    }
                }

                var collection = _db.Database.GetCollection<StudentItem>("Users");

                var query = subordinates == null ?
                    await collection.FindAsync(x =>
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                ) :
                  await collection.FindAsync(x =>
                    subordinates.Contains(x.Id) &&
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                );

                var students = await query.ToListAsync();

                if (students.Count == 0)
                    return track;

                var eventIds = track.EventsConfiguration.Select(x => x.EventId).ToList();
                var studentIds = students.Select(x => x.Id).ToList();

                var dbEventApplication = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => studentIds.Contains(x.UserId) && eventIds.Contains(x.EventId))
                    .ToListAsync();

                var events = await _db.EventCollection.AsQueryable()
                    .Where(x => eventIds.Contains(x.Id) &&
                           x.Requirements != null &&
                           x.Schedules != null &&
                           x.Requirements.Count > 0 &&
                           x.Schedules.Count > 0)
                    .Select(x => new { EventId = x.Id, x.Requirements, x.Schedules })
                    .ToListAsync();

                var eventRequirements = events
                    .SelectMany(x => x.Requirements.Select(r => new { x.EventId, r.ModuleId, r.Optional }))
                    .ToList();

                var eventModules = eventRequirements.Select(x => x.ModuleId).Distinct().ToArray();
                var trackStudents = students.Select(x => x.Id).ToArray();

                //Buscando o progresso e pontos por aluno
                var moduleUserProgressList = await _db.UserModuleProgressCollection.AsQueryable()
                    .Where(x => eventModules.Contains(x.ModuleId) && trackStudents.Contains(x.UserId))
                    .ToListAsync();
                var moduleUserProgress = moduleUserProgressList
                    .Select(x => new { x.ModuleId, Level = x.Level > 0 ? x.Level - 1 : 0, x.Points, x.UserId, x.Progress })
                    .ToList();

                track.LateStudents = new List<LateStudentItem>();

                foreach (var eventConfig in track.EventsConfiguration)
                {
                    eventConfig.IncompleteRequirementStudents = new List<ObjectId>();

                    eventConfig.Keys = keys;

                    eventConfig.Applications = dbEventApplication
                        .Where(x => x.EventId == eventConfig.EventId)
                        .Select(x => new TrackEventApplicationItem
                        {
                            UserId = x.UserId,
                            Name = students.FirstOrDefault(y => y.Id == x.UserId).Name,
                            ImageUrl = students.FirstOrDefault(y => y.Id == x.UserId).ImageUrl,
                            InorganicGrade = x.InorganicGrade,
                            OrganicGrade = x.OrganicGrade,
                            CustomEventGradeValues = x.CustomEventGradeValues
                        })
                        .ToList();

                    var dbEvent = events.FirstOrDefault(x => x.EventId == eventConfig.EventId);

                    if (dbEvent == null ||
                        dbEvent.Requirements == null || dbEvent.Requirements.Count == 0 ||
                        dbEvent.Schedules == null || dbEvent.Schedules.Count == 0)
                        continue;

                    var schedule = dbEvent.Schedules.FirstOrDefault(x =>
                        x.EventDate >= DateTimeOffset.Now &&
                        (x.EventDate - DateTimeOffset.Now).TotalDays < 7
                    );

                    foreach (var requirement in dbEvent.Requirements.Where(r => !r.Optional))
                    {
                        foreach (var student in students)
                        {
                            var moduleProgress = moduleUserProgress.FirstOrDefault(x =>
                                x.UserId == student.Id &&
                                x.ModuleId == requirement.ModuleId
                            );

                            var lateStudent = track.LateStudents.FirstOrDefault(x => x.StudentId == student.Id);

                            if (moduleProgress == null)
                            {
                                if (schedule != null)
                                {
                                    if (lateStudent == null)
                                    {
                                        track.LateStudents.Add(new LateStudentItem
                                        {
                                            StudentId = student.Id,
                                            Progress = 0,
                                            EventsTotal = 1,
                                            LateEvents = new List<ObjectId> { eventConfig.EventId }
                                        });
                                    }
                                    else
                                    {
                                        lateStudent.EventsTotal++;
                                        lateStudent.LateEvents.Add(eventConfig.EventId);
                                    }
                                }

                                if (!eventConfig.IncompleteRequirementStudents.Any(x => x == student.Id))
                                    eventConfig.IncompleteRequirementStudents.Add(student.Id);
                            }
                            else
                            {
                                decimal progress = (decimal)(moduleProgress.Level + 1) /
                                    (decimal)(requirement.RequirementValue.Level + 1);

                                if (progress > 1) { progress = 1; }

                                if (schedule != null)
                                {
                                    if (lateStudent == null && progress < 1)
                                    {
                                        track.LateStudents.Add(new LateStudentItem
                                        {
                                            StudentId = student.Id,
                                            Progress = progress,
                                            EventsTotal = 1,
                                            LateEvents = new List<ObjectId> { eventConfig.EventId }
                                        });
                                    }
                                    else if (progress < 1)
                                    {
                                        lateStudent.Progress = lateStudent.Progress + progress;
                                        lateStudent.EventsTotal++;
                                        lateStudent.LateEvents.Add(eventConfig.EventId);
                                    }
                                }

                                if (moduleProgress.Level > requirement.RequirementValue.Level ||
                                    (moduleProgress.Level == requirement.RequirementValue.Level &&
                                    moduleProgress.Progress >= requirement.RequirementValue.Percentage))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!eventConfig.IncompleteRequirementStudents.Any(x => x == student.Id))
                                        eventConfig.IncompleteRequirementStudents.Add(student.Id);
                                }
                            }
                        }
                    }
                }

                return track;
            }
        }
    }
}
