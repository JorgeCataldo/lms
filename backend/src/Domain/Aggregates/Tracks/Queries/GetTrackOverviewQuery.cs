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

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackOverviewQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public int Page { get; set; }
            public int PageSize { get; set; } = 10;
            public string SearchTerm { get; set; }
            public string TrackId { get; set; }
            public bool GetManageInfo { get; set; } = false;
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public int StudentsCount { get; set; }
            public StudentAchivmentsItem Student { get; set; }
            public List<StudentItem> Students { get; set; }
            public List<StudentPerformanceItem> TopPerformants { get; set; }
            public List<StudentPerformanceItem> BottomPerformants { get; set; }
            public decimal Duration { get; set; }
            public List<WrongConcept> WrongConcepts { get; set; }
            public List<StudentsProgressItem> StudentsProgress { get; set; }
            public bool IsStudent { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public bool IsBlocked { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<WrongConcept> WrongConcepts { get; set; }
            public RelationalItem BusinessGroup { get; set; }
            public RelationalItem BusinessUnit { get; set; }
            public RelationalItem Segment { get; set; }
        }

        public class StudentPerformanceItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public string BusinessGroup { get; internal set; }
            public string BusinessUnit { get; internal set; }
            public string Segment { get; internal set; }
        }

        public class StudentAchivmentsItem
        {
            public int AchievedGoals { get; set; }
            public int UnachievedGoals { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int Order { get; set; }
            public decimal ClassLevel { get; set; }
            public int CompleteStudents { get; set; }
            public List<UserProgressItem> Students { get; internal set; }
            public decimal Weight { get; set; }
            public DateTimeOffset? CutOffDate { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? CloseDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public decimal Weight { get; set; }
            public DateTimeOffset? CutOffDate { get; set; }
        }

        public class TrackItemWeight
        {
            public ObjectId ItemId { get; set; }
            public decimal Weight { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
            public bool AcheivedGoal { get; internal set; }
            public ObjectId UserId { get; internal set; }
            public ObjectId ModuleId { get; internal set; }
            public int Level { get; internal set; }
            public int Points { get; internal set; }
            public decimal Grade { get; internal set; }
            public string UserName { get; internal set; }
            public string ImageUrl { get; set; }
            public string BusinessGroup { get; internal set; }
            public string BusinessUnit { get; internal set; }
            public string Segment { get; internal set; }
        }

        public class StudentsProgressItem
        {
            public int? Level { get; set; }
            public int Count { get; set; }
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

                var trackId = ObjectId.Parse(request.TrackId);
                var track = await GetTrackById(trackId, cancellationToken);
                track.WrongConcepts = new List<WrongConcept>();

                if (track == null)
                    return Result.Fail<TrackItem>("Trilha não existe");

                if (request.UserRole == "Admin" || request.UserRole == "HumanResources" || request.UserRole == "Secretary")
                {
                    track = await GetTrackStudents(track, request, cancellationToken);

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

                    track = await GetTrackStudents(track, request, cancellationToken, responsible.SubordinatesUsersIds);
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

                        track.ModulesConfiguration = track.ModulesConfiguration.Where(x => InstructorModulesIds.Contains(x.ModuleId)).ToList();
                        track = await GetTrackStudents(track, request, cancellationToken);
                        return Result.Ok(track);
                    }
                    else
                    {
                        responsible.SubordinatesUsersIds.Add(userId);

                        track = await GetTrackStudents(track, request, cancellationToken, responsible.SubordinatesUsersIds);
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

            private async Task<TrackItem> GetTrackStudents(TrackItem track, Contract request, CancellationToken token, List<ObjectId> subordinates = null)
            {

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

                    foreach (TrackModuleItem trackModuleItem in track.ModulesConfiguration)
                    {
                        trackModuleItem.Weight = trackItemsWeight.FirstOrDefault(x => x.ItemId == trackModuleItem.ModuleId).Weight;
                    }
                }

                track.StudentsProgress = PopulateStudentsProgress();
                track.Student = new StudentAchivmentsItem
                {
                    AchievedGoals = 0,
                    UnachievedGoals = 0
                };

                var collection = _db.Database.GetCollection<StudentItem>("Users");

                var query = subordinates == null ?
                    await collection.FindAsync(x =>
                    x.IsBlocked == false &&
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                ) :
                  await collection.FindAsync(x =>
                    subordinates.Contains(x.Id) &&
                    x.IsBlocked == false &&
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == track.Id
                    )
                );

                var students = await query.ToListAsync();

                if (students.Count == 0)
                {
                    track.Students = new List<StudentItem>();
                    return track;
                }

                track.StudentsCount = students.Count();

                var userId = ObjectId.Parse(request.UserId);
                track.IsStudent = students.Any(s => s.Id == userId);

                if (request.GetManageInfo)
                {
                    var trackModules = track.ModulesConfiguration.Select(x => x.ModuleId).ToList();
                    var trackStudents = students.Select(x => x.Id).ToList();

                    var studentGrades = Module.GetModulesGrades(_db, trackStudents, trackModules).Data;

                    //Buscando o progresso e pontos por aluno
                    var moduleUserProgressList = await _db.UserModuleProgressCollection.AsQueryable()
                        .Where(x => trackModules.Contains(x.ModuleId) && trackStudents.Contains(x.UserId))
                        .ToListAsync();
                    var moduleUserProgress = moduleUserProgressList
                        .Select(x => new { x.ModuleId, x.Level, x.Points, x.UserId })
                        .ToList();

                    //Agrupando os badges dos alunos
                    track.StudentsProgress = moduleUserProgress.GroupBy(x => x.Level)
                        .Select(g => new StudentsProgressItem() { Level = g.Key, Count = g.Count() })
                        .ToList();

                    //Colocando os badges inexistentes
                    track.StudentsProgress.AddRange(PopulateStudentsProgress().Where(x => !track.StudentsProgress.Select(y => y.Level).Contains(x.Level)));

                    //Fazendo a contagem dos alunos que nao começaram ainda
                    var studentProgressed = moduleUserProgress.Select(x => x.UserId).ToArray();
                    track.Student.UnachievedGoals = students.Count(x => !studentProgressed.Contains(x.Id));

                    //Setando o numero de não iniciados no badge null
                    track.StudentsProgress.First(x => x.Level == null).Count = track.Student.UnachievedGoals;
                    track.StudentsProgress = track.StudentsProgress.OrderBy(x => x.Level ?? -1).ToList();

                    //Ordenando para visualização correta na interface
                    track.StudentsProgress = track.StudentsProgress.OrderBy(s => s.Level).ToList();

                    //Buscando os alunos que ja começaram e seus objetivos
                    var acheivedGoals =
                        from configs in track.ModulesConfiguration
                        join progress in moduleUserProgress on configs.ModuleId equals progress.ModuleId
                        join student in students on progress.UserId equals student.Id
                        join grade in studentGrades on configs.ModuleId equals grade.ModuleId
                        select new UserProgressItem()
                        {
                            UserId = progress.UserId,
                            UserName = student.Name,
                            ImageUrl = student.ImageUrl,
                            ModuleId = progress.ModuleId,
                            Level = progress.Level,
                            Points = progress.Points,
                            Grade = grade.UserGrades.FirstOrDefault(x => x.UserId == progress.UserId).Grade,
                            AcheivedGoal = configs.Level <= progress.Level,
                            BusinessGroup = student.BusinessGroup != null ? student.BusinessGroup.Name : "",
                            BusinessUnit = student.BusinessUnit != null ? student.BusinessUnit.Name : "",
                            Segment = student.Segment != null ? student.Segment.Name : ""
                        };

                    track.Student.AchievedGoals = acheivedGoals.Count(x => x.AcheivedGoal);
                    track.Student.UnachievedGoals += acheivedGoals.Count(x => x.AcheivedGoal);

                    foreach (var student in students)
                    {
                        student.Points = moduleUserProgress.Where(x => x.UserId == student.Id).Sum(x => x.Points);
                    }

                    foreach (var config in track.ModulesConfiguration)
                    {
                        var currentModuleStudents = acheivedGoals.Where(x => x.ModuleId == config.ModuleId);
                        config.CompleteStudents = currentModuleStudents.Count(x => x.AcheivedGoal);
                        config.Students = currentModuleStudents.ToList();
                        config.ClassLevel = currentModuleStudents.Count() > 0 ?
                            currentModuleStudents.Sum(x => x.Level) / (decimal)currentModuleStudents.Count() : 0;
                    }
                }

                var trackModulesId = track.ModulesConfiguration.Select(x => x.ModuleId).ToList();
                foreach (var student in students)
                {
                    if (student.WrongConcepts != null)
                    {
                        track.WrongConcepts.AddRange(
                            student.WrongConcepts.Where(x => trackModulesId.Contains(x.ModuleId))
                        );
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

                if (request.GetManageInfo)
                {
                    track.TopPerformants = students
                        .OrderByDescending(s => s.Points)
                        .Select(x => new StudentPerformanceItem()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            ImageUrl = x.ImageUrl,
                            Points = x.Points,
                            BusinessGroup = x.BusinessGroup != null ? x.BusinessGroup.Name : "",
                            BusinessUnit = x.BusinessUnit != null ? x.BusinessUnit.Name : "",
                            Segment = x.Segment != null ? x.Segment.Name : ""
                        })
                        .ToList();
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    students = students.Where(
                        x => x.Name.ToLower().Contains(request.SearchTerm.ToLower())
                    ).ToList();
                }

                track.Students = students
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return track;
            }

            private List<StudentsProgressItem> PopulateStudentsProgress()
            {
                var studentsProgress = new List<StudentsProgressItem>();

                studentsProgress.Add(new StudentsProgressItem
                {
                    Count = 0,
                    Level = null
                });
                studentsProgress.AddRange(Levels.Level.GetAllLevels().Data.Select(x => new StudentsProgressItem
                {
                    Count = 0,
                    Level = x.Id
                }).OrderBy(x => x.Level));

                return studentsProgress;
            }
        }
    }
}
