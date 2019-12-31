using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using System.Linq;

namespace Domain.Aggregates.Modules.Queries
{
    public class GetModuleOverviewQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
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
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public decimal Objective { get; set; }
            public bool Finished { get; set; }
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

        public class Handler : IRequestHandler<Contract, Result<TrackItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<TrackItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<TrackItem>("Id do Módulo não informado");
                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail<TrackItem>("Usuário não informado");

                var moduleId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);
                var track = new TrackItem();

                track = await GetConfigurations(track, moduleId, userId, cancellationToken);

                return Result.Ok(track);
                
            }

            private async Task<TrackItem> GetConfigurations(TrackItem track, ObjectId moduleId, ObjectId userId, CancellationToken token)
            {
                var collection = _db.Database.GetCollection<Student>("Users");

                var studentsQuery = 
                    await collection.FindAsync(
                        x => x.Id == userId
                    );

                var students = await studentsQuery.ToListAsync();
                List<Student> trackStudents;

                if (students.Count == 0)
                {
                    trackStudents = new List<Student>();
                }
                else
                {
                    trackStudents = students;
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

                track = await GetStudentsInfo(track, moduleId, trackStudents, progresses);

                return track;
            }

            private async Task<TrackItem> GetStudentsInfo(
                TrackItem track, ObjectId moduleId, List<Student> trackStudents, List<UserModuleProgress> progresses
            ) {
                track.Students = new List<StudentItem>();
                track.StudentsProgress = new List<StudentsProgressItem>();
                track.WrongConcepts = new List<WrongConcept>();
                track.ViewedContents = new List<ViewedContentItem>();

                track.StudentsProgress.Add(new StudentsProgressItem {
                    Count = 0, Level = null
                });
                track.StudentsProgress.Add(new StudentsProgressItem {
                    Count = 0, Level = 0
                });
                track.StudentsProgress.Add(new StudentsProgressItem {
                    Count = 0, Level = 1
                });
                track.StudentsProgress.Add(new StudentsProgressItem {
                    Count = 0, Level = 2
                });
                track.StudentsProgress.Add(new StudentsProgressItem {
                    Count = 0, Level = 3
                });

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
                            level = userProgress.Level > 0 ? userProgress.Level - 1 : 0;
                            track.StudentsProgress.First(x => x.Level == level).Count++;
                        }
                    }

                    track.Students.Add(new StudentItem {
                        Id = student.Id,
                        ImageUrl = student.ImageUrl,
                        Level = level,
                        Name = student.Name,
                        Objective = userProgress == null ? 0 : userProgress.Progress
                    });
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
                            track.ViewedContents.Add(new ViewedContentItem {
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
