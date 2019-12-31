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
using static Domain.Aggregates.Users.User;
using Domain.Aggregates.Users;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Events;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackStudentOverviewQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string TrackId { get; set; }
            public string StudentId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public StudentItem Student { get; set; }
            public int StudentsCount { get; set; }
            public decimal Duration { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public List<AttendedEventItem> AttendedEvents { get; set; }
            public int AchievedGoals { get; set; }
            public int UnachievedGoals { get; set; }
            public List<WrongConcept> WrongConcepts { get; set; }
            public int Points { get; set; } = 0;
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int StudentLevel { get; set; }
            public decimal ClassLevel { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
        }

        public class AttendedEventItem
        {
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public bool? Presence { get; set; }
            public decimal? FinalGrade { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
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
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && 
                    request.UserRole != "Student" && request.UserRole != "BusinessManager" && 
                    request.UserRole != "Secretary" && request.UserRole != "Recruiter")
                    return Result.Fail<TrackItem>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<TrackItem>("Id da Trilha não informado");
                if (String.IsNullOrEmpty(request.StudentId))
                    return Result.Fail<TrackItem>("Id do Aluno não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await GetTrackById(trackId, cancellationToken);
                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                var studentId = ObjectId.Parse(request.StudentId);
                var student = await GetStudentById(studentId, cancellationToken);
                if (student == null)
                    return Result.Fail<TrackItem>("Aluno não existe");

                if (request.UserRole == "Student" && request.UserId != request.StudentId)
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var responsible = await _db
                        .Database
                        .GetCollection<Responsible>("Responsibles")
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId && 
                            x.SubordinatesUsersIds.Contains(studentId));

                    if (responsible == null)
                    {
                        var InstructorEventsIds = await _db.EventCollection
                            .AsQueryable()
                            .Where(x => x.InstructorId == userId)
                            .Select(x => x.Id)
                            .ToListAsync(cancellationToken);

                        var InstructorModulesIds = await _db.ModuleCollection
                            .AsQueryable()
                            .Where(x => x.InstructorId == userId || x.ExtraInstructorIds.Contains(userId))
                            .Select(x => x.Id)
                            .ToListAsync(cancellationToken);

                        var isInstructor =
                            track.ModulesConfiguration.Any(x => InstructorModulesIds.Contains(x.ModuleId)) ||
                            track.EventsConfiguration.Any(x => InstructorEventsIds.Contains(x.EventId));

                        if (!isInstructor)
                            return Result.Fail<TrackItem>("Acesso Negado");
                    }
                }

                if (student.WrongConcepts != null)
                {
                    student.WrongConcepts = student.WrongConcepts.Where(c =>
                        track.ModulesConfiguration.Select(
                            m => m.ModuleId
                        ).Contains(c.ModuleId)
                    )
                    .OrderByDescending(wc => wc.Count)
                    .ToList();
                }
                else
                    student.WrongConcepts = new List<WrongConcept>();

                track.Student = await GetEventsParticipation(student, track.EventsConfiguration, cancellationToken);
                track = await GetConfigurations(track, cancellationToken);

                return Result.Ok(track);
            }

            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<StudentItem> GetStudentById(ObjectId studentId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<StudentItem>("Users")
                    .FindAsync(x => x.Id == studentId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetConfigurations(TrackItem track, CancellationToken token)
            {
                track.Student.AchievedGoals = 0;
                track.Student.UnachievedGoals = 0;
                
                var collection = _db.Database.GetCollection<Student>("Users");

                var studentsQuery = await collection.FindAsync(
                    x => x.TracksInfo != null
                );

                var students = await studentsQuery.ToListAsync();
                IEnumerable<ObjectId> trackStudentsIds;

                if (students.Count == 0)
                    trackStudentsIds = new List<ObjectId>();
                else
                {
                    var trackStudents = students.Where(s =>
                        s.TracksInfo != null &&
                        s.TracksInfo.Any(
                            y => y.Id == track.Id
                        )
                    );
                    
                    trackStudentsIds = trackStudents.Select(t => t.Id);
                }

                foreach (var config in track.ModulesConfiguration)
                {
                    var query = await _db.Database
                        .GetCollection<Module>("Modules")
                        .FindAsync(x =>
                            x.Id == config.ModuleId,
                            cancellationToken: token
                        );
                    
                    var progressColl = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                    var progQuery = await progressColl.FindAsync(x =>
                        x.ModuleId == config.ModuleId
                    );

                    var progresses = await progQuery.ToListAsync();
                    progresses = progresses.Where(p =>
                        trackStudentsIds.Contains(p.UserId)
                    ).ToList();

                    if (progresses.Count == 0)
                    {
                        config.StudentLevel = 0;
                        config.ClassLevel = 0;
                        track.Student.UnachievedGoals++;
                    }
                    else
                    {
                        config.ClassLevel = (decimal)progresses.Sum(x => x.Level) / (decimal)track.StudentsCount;

                        var studentProgress = progresses.FirstOrDefault(x =>
                            x.UserId == track.Student.Id
                        );

                        if (studentProgress == null)
                        {
                            config.StudentLevel = 0;
                            track.Student.UnachievedGoals++;
                        }
                        else
                        {
                            track.Student.Points = track.Student.Points + studentProgress.Points;
                            config.StudentLevel = studentProgress.Level;

                            if (studentProgress.Level > config.Level)
                                track.Student.AchievedGoals++;
                            else
                                track.Student.UnachievedGoals++;
                        }
                    }
                }

                return track;
            }

            private async Task<StudentItem> GetEventsParticipation(
                StudentItem student, List<TrackEventItem> events, CancellationToken token
            ) {
                student.AttendedEvents = new List<AttendedEventItem>();

                if (events != null)
                {
                    foreach (var item in events)
                    {
                        var application = await GetApplication(student.Id, item.EventScheduleId, token);
                        if (application != null)
                        {
                            student.AttendedEvents.Add(new AttendedEventItem
                            {
                                EventDate = application.EventDate,
                                EventScheduleId = application.ScheduleId,
                                FinalGrade = GetFinalGrade(application),
                                Presence = application.UserPresence,
                                Title = item.Title
                            });
                        }
                    }
                }

                return student;
            }

            private async Task<EventApplication> GetApplication(
                ObjectId studentId, ObjectId scheduleId, CancellationToken token
            ) {
                var query = await _db.Database
                        .GetCollection<EventApplication>("EventApplications")
                        .FindAsync(x =>
                            x.UserId == studentId && x.ScheduleId == scheduleId,
                            cancellationToken: token
                        );

                return await query.FirstOrDefaultAsync(cancellationToken: token);
            }

            private decimal? GetFinalGrade(EventApplication application)
            {
                if (application.InorganicGrade.HasValue && application.OrganicGrade.HasValue)
                {
                    var sum = application.InorganicGrade.Value + application.OrganicGrade.Value;
                    return Math.Round(sum / 2, 2);
                }
                return null;
            }
        }
    }
}
