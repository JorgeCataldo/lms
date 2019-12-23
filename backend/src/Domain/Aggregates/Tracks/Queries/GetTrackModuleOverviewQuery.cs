using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Users;
using Domain.Aggregates.UserProgressHistory;
using Action = Domain.Aggregates.Modules.Action;
using Domain.Aggregates.UserFiles;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackModuleOverviewQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string TrackId { get; set; }
            public string ModuleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ModuleTitle { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<WrongConcept> WrongConcepts { get; set; }
            public List<StudentsProgressItem> StudentsProgress { get; set; }
            public List<StudentItem> Students { get; set; }
            public List<ViewedContentItem> ViewedContents { get; set; }
            public List<SubjectsProgressItem> SubjectsProgress { get; set; }
            public List<UserFile> UserFiles { get; set; }
            public int StudentsCount { get; set; }
            public decimal Duration { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public int? Level { get; set; }
            public decimal Objective { get; set; }
            public bool Finished { get; set; }
            public UserFile UserFiles { get; set; }
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
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
        }

        public class StudentsProgressItem
        {
            public int? Level { get; set; }
            public int Count { get; set; }
        }

        public class ViewedContentItem
        {
            public string ContentTitle { get; set; }
            public ContentType ContentType { get; set; }
            public long Count { get; set; }
        }

        public class SubjectsProgressItem
        {
            public string SubjectTitle { get; set; }
            public decimal Level { get; set; }
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
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<TrackItem>("Id do Módulo não informado");

                var trackId = ObjectId.Parse(request.TrackId);
                var moduleId = ObjectId.Parse(request.ModuleId);

                var track = await GetTrackById(trackId, cancellationToken);
                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                if (request.UserRole == "Student")
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
                            var instructorModule = await _db.ModuleCollection
                                .AsQueryable()
                                .FirstOrDefaultAsync(x => (x.InstructorId == userId || x.ExtraInstructorIds.Contains(userId) || x.TutorsIds.Contains(userId)) && x.Id == moduleId);

                        if (instructorModule == null)
                            return Result.Fail<TrackItem>("Acesso Negado");

                        track = await GetConfigurations(track, moduleId, cancellationToken);
                    }
                    else
                    {
                        track = await GetConfigurations(track, moduleId, cancellationToken, responsible.SubordinatesUsersIds);
                    }
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

                    track = await GetConfigurations(track, moduleId, cancellationToken, responsible.SubordinatesUsersIds);
                }
                else
                {
                    track = await GetConfigurations(track, moduleId, cancellationToken);
                }

                return Result.Ok(track);
                
            }

            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetConfigurations(TrackItem track, ObjectId moduleId, CancellationToken token, List<ObjectId> subordinates = null)
            {
                var collection = _db.Database.GetCollection<Student>("Users");

                var studentsQuery = subordinates == null ?
                    await collection.FindAsync(
                        x => x.TracksInfo != null
                    ) :
                    await collection.FindAsync(
                        x => subordinates.Contains(x.Id) &&
                        x.TracksInfo != null
                    );

                var students = await studentsQuery.ToListAsync();
                List<Student> trackStudents;

                if (students.Count == 0)
                    trackStudents = new List<Student>();
                else
                {
                    trackStudents = students.Where(s =>
                        s.IsBlocked == false &&
                        s.TracksInfo.Any(
                            y => y.Id == track.Id
                        )
                    ).ToList();
                }

                var progressColl = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                var progQuery = await progressColl.FindAsync(x =>
                    x.ModuleId == moduleId
                );

                var progresses = await progQuery.ToListAsync();
                progresses = progresses.Where(p =>
                    trackStudents
                        .Select(s => s.Id)
                        .Contains(p.UserId)
                ).ToList();

                var userFiles = await _db.Database.GetCollection<UserFile>("UserFiles")
                    .AsQueryable()
                    .Where(x => x.ResourceId == moduleId).ToListAsync();

                track = await GetStudentsInfo(track, moduleId, trackStudents, progresses, userFiles);

                return track;
            }

            private async Task<TrackItem> GetStudentsInfo(
                TrackItem track, ObjectId moduleId, List<Student> trackStudents, List<UserModuleProgress> progresses, List<UserFile> userFiles
            )
            {
                track.Students = new List<StudentItem>();
                track.StudentsProgress = new List<StudentsProgressItem>();
                track.WrongConcepts = new List<WrongConcept>();
                track.ViewedContents = new List<ViewedContentItem>();
                track.SubjectsProgress = new List<SubjectsProgressItem>();
                track.UserFiles = new List<UserFile>();

                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = null
                });
                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = 0
                });
                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = 1
                });
                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = 2
                });
                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = 3
                });
                track.StudentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = 4
                });

                var trackModule = track.ModulesConfiguration.First(m =>
                    m.ModuleId == moduleId
                );

                track.ModulesConfiguration = new List<TrackModuleItem>
                {
                    trackModule
                };

                foreach (var student in trackStudents)
                {
                    if (student.WrongConcepts != null && student.WrongConcepts.Any(x => x.ModuleId == moduleId))
                    {
                        track.WrongConcepts.AddRange(
                            student.WrongConcepts.Where(x => x.ModuleId == moduleId).ToList()
                        );
                    }

                    UserModuleProgress userProgress = null;
                    int level = 0;
                    if (progresses.Count > 0)
                    {
                        userProgress = progresses.FirstOrDefault(p =>
                            p.UserId == student.Id
                        );

                        if (userProgress == null)
                            track.StudentsProgress.First(x => x.Level == null).Count++;
                        else
                        {
                            level = userProgress.Level;
                            track.StudentsProgress.First(x => x.Level == level).Count++;
                        }
                    }
                    
                    var newStud = new StudentItem
                    {
                        Id = student.Id,
                        ImageUrl = student.ImageUrl,
                        Name = student.Name,
                        Objective = userProgress == null ? 0 : userProgress.Progress,
                        Finished = userProgress != null && userProgress.Level > trackModule.Level,
                        UserFiles = userFiles.Where(x => x.CreatedBy == student.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefault()
                    };

                    if (userProgress != null)
                        newStud.Level = level;

                    track.Students.Add(newStud);
                }

                var dbModule = await GetModuleById(moduleId);
                track.ModuleTitle = dbModule.Title;

                foreach (var subject in dbModule.Subjects)
                {
                    var progressColl = _db.Database.GetCollection<UserSubjectProgress>("UserSubjectProgress");
                    var progQuery = await progressColl.FindAsync(x =>
                        x.ModuleId == moduleId
                    );
                    var subjectProgresses = await progQuery.ToListAsync();
                    decimal subjectProgressSum = subjectProgresses.Where(p =>
                        p.SubjectId == subject.Id &&
                        trackStudents
                            .Select(s => s.Id)
                            .Contains(p.UserId)
                    ).Sum(p => p.Level);

                    track.StudentsCount = trackStudents.Count();

                    if (track.StudentsCount > 0)
                    {
                        track.SubjectsProgress.Add(new SubjectsProgressItem
                        {
                            Level = subjectProgressSum / (decimal)track.StudentsCount,
                            SubjectTitle = subject.Title
                        });
                    }

                    foreach (var content in subject.Contents)
                    {
                        string contentId = content.Id.ToString();
                        var studentsIds = trackStudents.Select(s => s.Id);

                        var actionsColl = _db.Database.GetCollection<Action>("Actions");
                        var actionsQuery = await actionsColl.FindAsync(x =>
                            x.ContentId == contentId &&
                            x.Description == "content-access" &&
                            studentsIds.Contains(x.CreatedBy)
                        );
                        var actions = await actionsQuery.ToListAsync();
                        var actionsCount = actions.GroupBy(c => c.CreatedBy).Count();

                        if (actionsCount > 0)
                        {
                            track.ViewedContents.Add(new ViewedContentItem
                            {
                                ContentTitle = content.Title,
                                ContentType = content.Type,
                                Count = actionsCount
                            });
                        }
                    }
                }

                track.WrongConcepts = track.WrongConcepts
                    .GroupBy(c => new { c.Concept, c.ModuleId })
                    .Select(g => new WrongConcept
                    {
                        Concept = g.First().Concept,
                        ModuleId = g.First().ModuleId,
                        ModuleName = g.First().ModuleName,
                        Count = g.Sum(c => c.Count)
                    })
                    .OrderByDescending(wc => wc.Count)
                    .ToList();

                return track;
            }

            private async Task<Module> GetModuleById(ObjectId moduleId)
            {
                var qry = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == moduleId);

                return await qry.FirstOrDefaultAsync();
            }
        }
    }
}
