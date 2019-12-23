using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.Modules;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserSkillsQuery
    {
        public class Contract : CommandContract<Result<List<TrackItem>>>
        {
            public string StudentId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<StudentsProgressItem> StudentsProgress { get; set; }
            public StudentAchivmentsItem StudentAchivmentsItem { get; set; }
            public List<StudentPerformanceItem> TopPerformants { get; set; }
            public int StudentsCount { get; set; }
            public decimal ModulesGrade { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public int Points { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<WrongConcept> WrongConcepts { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
            public bool AcheivedGoal { get; internal set; }
            public ObjectId UserId { get; internal set; }
            public ObjectId ModuleId { get; internal set; }
            public int Level { get; internal set; }
            public int Points { get; internal set; }
            public string UserName { get; internal set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int StudentLevel { get; set; }
            public decimal ClassLevel { get; set; }
            public int CompleteStudents { get; set; }
            public List<UserProgressItem> Students { get; internal set; }
        }

        public class ModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public decimal MaxPoints { get; set; }
        }

        public class StudentsProgressItem
        {
            public int? Level { get; set; }
            public int Count { get; set; }
        }

        public class StudentAchivmentsItem
        {
            public int AchievedGoals { get; set; }
            public int UnachievedGoals { get; set; }
        }

        public class StudentPerformanceItem
        {
            public ObjectId Id { get; set; }
            public int Points { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<TrackItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<TrackItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" && request.UserId != request.StudentId)
                    return Result.Fail<List<TrackItem>>("Acesso Negado");
                
                if (String.IsNullOrEmpty(request.StudentId))
                    return Result.Fail<List<TrackItem>>("Id do Aluno não informado");
                
                var studentId = ObjectId.Parse(request.StudentId);

                var student = await GetStudentById(studentId, cancellationToken);
                if (student == null)
                    return Result.Fail<List<TrackItem>>("Aluno não existe");

                var trackItems = new List<TrackItem>();

                if (student.TracksInfo != null)
                {
                    var tracks = await GetStudentTracks(student.TracksInfo, cancellationToken);
                    var trackStudentsIds = await GetTracksStudentsIds(tracks, cancellationToken);

                    foreach (var track in tracks)
                    {
                        var trackItem = await GetConfigurations(studentId, track, trackStudentsIds, cancellationToken);
                        trackItems.Add(trackItem);
                    }
                }

                return Result.Ok(trackItems);
            }

            private async Task<List<Track>> GetStudentTracks(List<UserProgress> tracksInfo, CancellationToken token)
            {
                var tracksIds = tracksInfo.Select(t => t.Id);

                var query = _db.TrackCollection.AsQueryable()
                    .Where(t => tracksIds.Contains(t.Id));

                return await ((IAsyncCursorSource<Track>)query).ToListAsync();
            }

            private async Task<User> GetStudentById(ObjectId studentId, CancellationToken token)
            {
                return await _db.UserCollection.AsQueryable()
                    .Where(x => x.Id == studentId)
                    .FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<List<ObjectId>> GetTracksStudentsIds(List<Track> tracks, CancellationToken token)
            {
                var tracksIds = tracks.Select(t => t.Id);
                
                var trackStudentsIdsQuery = _db.UserCollection.AsQueryable()
                    .Where(s =>
                        s.TracksInfo != null &&
                        s.TracksInfo.Any(
                            y => tracksIds.Contains(y.Id)
                        )
                    )
                    .Select(t => t.Id);

                return await ((IAsyncCursorSource<ObjectId>)trackStudentsIdsQuery)
                    .ToListAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetConfigurations(
                ObjectId studentId, Track track, List<ObjectId> trackStudentsIds, CancellationToken token
            ) {
                var trackItem = new TrackItem() {
                    Id = track.Id,
                    Title = track.Title,
                    StudentsCount = track.StudentsCount,
                    ModulesConfiguration = track.ModulesConfiguration.Select(
                        m => new TrackModuleItem() {
                            Title = m.Title,
                            ClassLevel = 0,
                            StudentLevel = 0,
                            Level = m.Level,
                            ModuleId = m.ModuleId,
                            Percentage = m.Percentage
                        }
                    ).ToList()
                };

                var trackModulesIds = track.ModulesConfiguration.Select(m => m.ModuleId);

                var modulesProgressesQuery = _db.UserModuleProgressCollection.AsQueryable()
                    .Where(x =>
                        trackStudentsIds.Contains(x.UserId) &&
                        trackModulesIds.Contains(x.ModuleId)
                    );

                var modules = _db.ModuleCollection
                    .AsQueryable()
                    .Where(x => trackModulesIds.Contains(x.Id))
                    .Select(
                        x => new ModuleItem() {
                            ModuleId = x.Id,
                            MaxPoints = x.Subjects.Count() * 400
                        }
                    ).ToList();

                var modulesProgresses = await ((IAsyncCursorSource<UserModuleProgress>)modulesProgressesQuery).ToListAsync();

                trackItem.ModulesGrade = 0;

                foreach (var config in trackItem.ModulesConfiguration)
                {
                    var progresses = modulesProgresses.Where(m => m.ModuleId == config.ModuleId);

                    if (progresses.Count() == 0)
                    {
                        config.StudentLevel = 0;
                        config.ClassLevel = 0;
                    }
                    else
                    {
                        config.ClassLevel = (decimal)progresses.Sum(x => x.Level) / (decimal)track.StudentsCount;

                        var studentProgress = progresses.FirstOrDefault(x =>
                            x.UserId == studentId
                        );

                        var currentModule = modules.FirstOrDefault(x => x.ModuleId == config.ModuleId);
                        if (currentModule != null && studentProgress != null)
                        {
                            trackItem.ModulesGrade = trackItem.ModulesGrade + ((studentProgress.Points / currentModule.MaxPoints) * 10);
                        }

                        config.StudentLevel = studentProgress == null ? 0 : studentProgress.Level;
                    }
                }

                if (trackItem.ModulesConfiguration != null && trackItem.ModulesConfiguration.Count() != 0)
                    trackItem.ModulesGrade = trackItem.ModulesGrade / (decimal)trackItem.ModulesConfiguration.Count();
                else
                    trackItem.ModulesGrade = 0;

               // CALCULO DA POSIÇÃO DOS 3 MELHORES

               trackItem.StudentsProgress = PopulateStudentsProgress();
                trackItem.StudentAchivmentsItem = new StudentAchivmentsItem
                {
                    AchievedGoals = 0,
                    UnachievedGoals = 0
                };

                var collection = _db.Database.GetCollection<StudentItem>("Users");

                var query = await collection.FindAsync(x =>
                    x.TracksInfo != null &&
                    x.TracksInfo.Any(y =>
                        y.Id == trackItem.Id
                    )
                );

                var students = await query.ToListAsync();

                //Buscando o progresso e pontos por aluno
                var moduleUserProgressList = _db.UserModuleProgressCollection.AsQueryable()
                    .Where(x => trackModulesIds.Contains(x.ModuleId) && trackStudentsIds.Contains(x.UserId))
                    .ToList();

                var moduleUserProgress = moduleUserProgressList
                    .Select(x => new { x.ModuleId, Level = x.Level > 0 ? x.Level - 1 : 0, x.Points, x.UserId })
                    .ToList();

                //Agrupando os badges dos alunos
                trackItem.StudentsProgress = moduleUserProgress.GroupBy(x => x.Level)
                    .Select(g => new StudentsProgressItem() { Level = g.Key, Count = g.Count() })
                    .ToList();

                //Colocando os badges inexistentes
                trackItem.StudentsProgress.AddRange(PopulateStudentsProgress().Where(x => !trackItem.StudentsProgress.Select(y => y.Level).Contains(x.Level)));

                //Fazendo a contagem dos alunos que nao começaram ainda
                var studentProgressed = moduleUserProgress.Select(x => x.UserId).ToArray();
                trackItem.StudentAchivmentsItem.UnachievedGoals = students.Count(x => !studentProgressed.Contains(x.Id));

                //Setando o numero de não iniciados no badge null
                trackItem.StudentsProgress.First(x => x.Level == null).Count = trackItem.StudentAchivmentsItem.UnachievedGoals;

                //Ordenando para visualização correta na interface
                trackItem.StudentsProgress = trackItem.StudentsProgress.OrderBy(s => s.Level).ToList();

                //Buscando os alunos que ja começaram e seus objetivos
                var acheivedGoals =
                    from configs in trackItem.ModulesConfiguration
                    join progress in moduleUserProgress on configs.ModuleId equals progress.ModuleId
                    join student in students on progress.UserId equals student.Id
                    select new UserProgressItem()
                    {
                        UserId = progress.UserId,
                        ModuleId = progress.ModuleId,
                        Level = progress.Level,
                        Points = progress.Points,
                        AcheivedGoal = configs.Level < progress.Level
                    };

                trackItem.StudentAchivmentsItem.AchievedGoals = acheivedGoals.Count(x => x.AcheivedGoal);
                trackItem.StudentAchivmentsItem.UnachievedGoals += acheivedGoals.Count(x => x.AcheivedGoal);

                foreach (var student in students)
                {
                    student.Points = moduleUserProgress.Where(x => x.UserId == student.Id).Sum(x => x.Points);
                }

                foreach (var config in trackItem.ModulesConfiguration)
                {
                    var currentModuleStudents = acheivedGoals.Where(x => x.ModuleId == config.ModuleId);
                    config.CompleteStudents = currentModuleStudents.Count(x => x.AcheivedGoal);
                    config.Students = currentModuleStudents.ToList();
                    config.ClassLevel = currentModuleStudents.Count() > 0 ?
                        currentModuleStudents.Sum(x => x.Level) / (decimal)currentModuleStudents.Count() : 0;
                }

                trackItem.TopPerformants = students
                    .Where(s => s.Points > 0)
                    .OrderByDescending(s => s.Points)
                    .Take(3)
                    .Select(x => new StudentPerformanceItem()
                    {
                        Id = x.Id,
                        Points = x.Points
                    })
                    .ToList();

                return trackItem;
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
