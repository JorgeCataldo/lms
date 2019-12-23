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
using Domain.Aggregates.ValuationTests;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackStudentOverviewQuery
    {
        public class Contract : CommandContract<Result<List<StudentItem>>>
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
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<StudentItem> Students { get; set; }
            public int StudentsCount { get; set; }
            public decimal Duration { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public decimal Level { get; set; }
            public int Points { get; set; } = 0;
            public decimal CurrentProgress { get; set; }
            public List<ProgressItem> ProgressList { get; set; }
            public decimal FinalGrade { get; set; }
            public List<UserModuleItem> Modules { get; set; }
            public List<UserEventItem> Events { get; set; }
        }

        public class UserEventItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId EventId { get; set; }
            public string EventName { get; set; }
            public decimal? FinalGrade { get; set; }
            public decimal? WeightGrade { get; set; }
            public DateTimeOffset? Date { get; set; }
        }

        public class ProgressItem
        {
            public ObjectId ModuleId { get; set; }
            public decimal Progress { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int StudentLevel { get; set; }
            public decimal ClassLevel { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
            public  int Weight { get; set; }
        }


        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public string Title { get; set; }
            public int Weight { get; set; }
        }

        public class UserModuleItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId ModuleId { get; set; }
            public int CorrectAnswers { get; set; }
            public int TotalAnswers { get; set; }
            public decimal EvaluationGrade { get; set; }
            public decimal Progress { get; set; }
            public decimal BdqWeight { get; set; }
            public decimal EvaluationWeight { get; set; }
            public decimal ModuleGrade { get; set; }
            public decimal ModuleWeight { get; set; }
            public string Title { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<StudentItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<StudentItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "BusinessManager")
                    return Result.Fail<List<StudentItem>>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TrackId))
                    return Result.Fail<List<StudentItem>>("Id da Trilha não informado");
                
                var trackId = ObjectId.Parse(request.TrackId);
                var track = await GetTrackById(trackId, cancellationToken);
                if (track == null)
                    return Result.Fail<List<StudentItem>>("Trilha não existe");

                track.Students = await GetStudents(trackId);

                var trackStudentsIds = track.Students.Select(x => x.Id).ToList();
                var modulesIds = track.ModulesConfiguration.Select(x => x.ModuleId).ToList();
                var eventIds = new List<ObjectId>();

                if (track.EventsConfiguration != null)
                {
                    eventIds = track.EventsConfiguration.Select(x => x.EventId).ToList();
                }

                track = await GetConfigurations(track, cancellationToken);
                var modulesGrades = await GetGrades(modulesIds, eventIds, trackStudentsIds, cancellationToken);
                var eventsGrades = await GetEventGrades(eventIds, trackStudentsIds, cancellationToken);

                var modules = await _db.ModuleCollection
                    .AsQueryable()
                    .Where(x => modulesIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                var valuationTests = await _db.ValuationTestCollection
                           .AsQueryable()
                           .Where(x => x.TestTracks.Any(y => modulesIds.Contains(y.Id)))
                           .ToListAsync(cancellationToken);

                var openedModules = track.ModulesConfiguration.Where(x => x.OpenDate <= DateTimeOffset.Now).ToList();
                
                for (int i = 0; i < track.Students.Count; i++)
                {
                    track.Students[i].Modules = modulesGrades.Where(x => x.UserId == track.Students[i].Id).ToList();
                    track.Students[i].Events = eventsGrades.Where(x => x.UserId == track.Students[i].Id).ToList();

                    for (int k = 0; k < track.Students[i].Modules.Count; k++)
                    {
                        var weights = modules.Where(x => x.Id == track.Students[i].Modules[k].ModuleId).Select(x => x.ModuleWeights).FirstOrDefault();
                        track.Students[i].Modules[k].EvaluationGrade = 0;
                        track.Students[i].Modules[k].ModuleGrade = 0;
                        track.Students[i].Modules[k].OpenDate = track.ModulesConfiguration.Where(x => x.ModuleId == track.Students[i].Modules[k].ModuleId)
                            .FirstOrDefault().OpenDate;
                        track.Students[i].Modules[k].ValuationDate = track.ModulesConfiguration.Where(x => x.ModuleId == track.Students[i].Modules[k].ModuleId)
                            .FirstOrDefault().ValuationDate;

                        var moduleConfiguration = track.ModulesConfiguration.Where(x => x.ModuleId == track.Students[i].Modules[k].ModuleId).FirstOrDefault();
                        
                        decimal moduleBdqGrade = 0;
                        decimal moduleEvaluationqGrade = 0;
                        track.Students[i].Modules[k].ModuleWeight = (moduleConfiguration.Weight * track.ModulesConfiguration.Sum(x => x.Weight))
                            / openedModules.Sum(x => x.Weight);

                        var progressItem = track.Students[i].ProgressList.Where(x => x.ModuleId == track.Students[i].Modules[k].ModuleId).FirstOrDefault();

                        if (track.Students[i].Modules[k].TotalAnswers > 0)                        
                        {
                            if(progressItem == null)
                            {
                                track.Students[i].ProgressList.Add(new ProgressItem {
                                    ModuleId = track.Students[i].Modules[k].ModuleId,
                                    Progress = (decimal)0.3
                                });

                                track.Students[i].CurrentProgress = (decimal)track.Students[i].ProgressList.Sum(x => x.Progress) / openedModules.Count * 100;
                            }

                            var level = progressItem == null || progressItem.Progress == 0 ? (decimal)0.3 : progressItem.Progress;
                            track.Students[i].Modules[k].Progress = level;
                            moduleBdqGrade = ((decimal)track.Students[i].Modules[k].CorrectAnswers / (decimal)track.Students[i].Modules[k].TotalAnswers * level * 100);
                        }
                        else
                        {
                            track.Students[i].Modules[k].Progress = progressItem != null ? progressItem.Progress : 0;
                        }

                        if(valuationTests != null && valuationTests.Count > 0)
                        {
                            var studentModuleTestId = valuationTests.Where(x => x.TestModules.Any(y => y.Id == track.Students[i].Modules[k].ModuleId))
                                    .Select(x => x.Id).FirstOrDefault();

                            var valuationTest = await _db.ValuationTestResponseCollection
                           .AsQueryable()
                           .Where(x => x.Id == studentModuleTestId && x.CreatedBy == track.Students[i].Id)
                           .FirstOrDefaultAsync(cancellationToken);

                            if(valuationTest != null)
                            {
                                decimal totalWeight = 0;

                                for (int j = 0; j < valuationTest.Answers.Count; j++)
                                {
                                    totalWeight += (decimal)valuationTest.Answers[j].Percentage;
                                    moduleEvaluationqGrade = (decimal)valuationTest.Answers[j].Grade * (decimal)valuationTest.Answers[j].Percentage;
                                }

                                moduleEvaluationqGrade = moduleEvaluationqGrade / totalWeight;
                            }
                        }

                        if (weights != null)
                        {
                            track.Students[i].Modules[k].ModuleGrade = (moduleBdqGrade * weights[1].Weight + moduleEvaluationqGrade * weights[2].Weight) / (weights[1].Weight + weights[2].Weight);
                        }

                        track.Students[i].FinalGrade += (track.Students[i].Modules[k].ModuleGrade * track.Students[i].Modules[k].ModuleWeight);
                    }

                    track.Students[i].FinalGrade = track.Students[i].FinalGrade / 100;
                }

                return Result.Ok(track.Students);
            }

            private async Task<TrackItem> GetTrackById(ObjectId trackId, CancellationToken token)
            {
                var qry = await _db.Database
                    .GetCollection<TrackItem>("Tracks")
                    .FindAsync(x => x.Id == trackId, cancellationToken: token);

                return await qry.FirstOrDefaultAsync(cancellationToken: token);
            }

            private async Task<TrackItem> GetConfigurations(TrackItem track, CancellationToken token)
            {
                var i = 0;
                var k = 0;
                var j = 0;

                try
                {
                    var trackStudentsIds = track.Students.Select(x => x.Id).ToList();
                    var userProgresses = new List<UserModuleProgress>();
                    var userBdqProgresses = new List<UserSubjectProgress>();

                    var openedModules = track.ModulesConfiguration.Count(x => x.OpenDate <= DateTime.Today);
                    var modulesIds = track.ModulesConfiguration.Select(x => x.ModuleId);

                    foreach (var config in track.ModulesConfiguration)
                    {
                        if (config.OpenDate <= DateTime.Today)
                        {

                            var progressColl = _db.Database.GetCollection<UserModuleProgress>("UserModuleProgress");
                            var progQuery = await progressColl.FindAsync(x =>
                                x.ModuleId == config.ModuleId
                            );

                            var progresses = await progQuery.ToListAsync();
                            progresses = progresses.Where(p =>
                                trackStudentsIds.Contains(p.UserId)
                            ).ToList();

                            userProgresses = userProgresses.Concat(progresses).ToList();
                        }
                    }
                                        
                    for (i = 0; i < track.Students.Count; i++)
                    {
                        for (k = 0; k < userProgresses.Count; k++)
                        {
                            if (track.Students[i].Id == userProgresses[k].UserId)
                            {
                                decimal progress = 0;

                                switch (userProgresses[k].Level)
                                {
                                    case 1:
                                        progress = (decimal)0.3;
                                        break;
                                    case 2:
                                        progress = (decimal)0.6;
                                        break;
                                    case 3:
                                        progress = (decimal)0.9;
                                        break;
                                    case 4:
                                        progress = 1;
                                        break;
                                    default:
                                        progress = 0;
                                        break;
                                }

                                track.Students[i].ProgressList.Add(new ProgressItem {
                                    ModuleId = userProgresses[k].ModuleId,
                                    Progress = progress
                                });
                            }
                        }

                        if (track.Students[i].ProgressList.Count > 0)
                        {
                            track.Students[i].CurrentProgress = (decimal)track.Students[i].ProgressList.Sum(x => x.Progress) / openedModules * 100;
                        }
                    }
                }
                catch(Exception ex)
                {
                    var errI = i;
                    var errK = k;
                    Console.WriteLine(ex.Message);
                    return new TrackItem();
                }

                return track;
            }

            private async Task<List<UserModuleItem>> GetGrades(List<ObjectId> modulesIds, List<ObjectId> eventIds, List<ObjectId> studentsIds, CancellationToken token)
            {
                var i = 0;
                var k = 0;
                var b = 0;
                try
                {
                    var collection = _db.Database.GetCollection<Module>("Modules");
                    var moduleQuery = await collection.FindAsync(
                        x => modulesIds.Contains(x.Id)
                    );

                    var modulesList = await moduleQuery.ToListAsync();
                    var bdqProgressBase = _db.Database.GetCollection<UserSubjectProgress>("UserSubjectProgress");
                    var bdqProgressQuery = await bdqProgressBase.FindAsync(x =>
                        modulesIds.Contains(x.ModuleId) &&
                        studentsIds.Contains(x.UserId)
                    );

                    
                    var userModuleList = new List<UserModuleItem>();
                    var userBdqProgresses = await bdqProgressQuery.ToListAsync();

                    for (i = 0; i < studentsIds.Count; i++)
                    {
                        for (k = 0; k < modulesList.Count; k++)
                        {
                            userModuleList.Add(new UserModuleItem
                            {
                                UserId = studentsIds[i],
                                ModuleId = modulesList[k].Id,
                                Title = modulesList[k].Title
                            });

                            var userModuleBdqProgress = userBdqProgresses.Where(x => x.ModuleId == modulesList[k].Id
                                && x.UserId == studentsIds[i]).ToList();
                            var userBdqTotalAnswers = userModuleBdqProgress.GroupBy(x => x.UserId).Select(gp => new
                            {
                                TotalAnswers = gp.Sum(x => x.Answers.Count())
                            }).FirstOrDefault();

                            for (b = 0; b < userModuleBdqProgress.Count; b++)
                            {
                                userModuleList[i].CorrectAnswers += userModuleBdqProgress[b].Answers.Where(a => a.CorrectAnswer == true).Count();
                                userModuleList[i].TotalAnswers += userModuleBdqProgress[b].Answers.Count();
                            }
                        }
                    }
                                        
                    return userModuleList;
                }
                catch(Exception ex)
                {
                    var testei = i;
                    var testek = k;
                    var testeb = b;
                    return new List<UserModuleItem>();
                }
            }

            private async Task<List<UserEventItem>> GetEventGrades(List<ObjectId> eventIds, List<ObjectId> studentsIds, CancellationToken token)
            {
                var i = 0;
                var k = 0;
                var b = 0;
                try
                {
                    var userEventList = new List<UserEventItem>();
                    var eventApplications = await GetApplications(studentsIds, eventIds, token);
                    var events = await _db.EventCollection
                           .AsQueryable()
                           .Where(x => eventIds.Contains(x.Id))
                           .ToListAsync(token);


                    for (i = 0; i < studentsIds.Count; i++)
                    {
                        for (k = 0; k < events.Count; k++)
                        {
                            decimal finalGradeValue = 0;

                            var application = eventApplications.Where(x => x.UserId == studentsIds[i] && x.EventId == events[k].Id).FirstOrDefault();

                            var basevalues = application != null && application.GradeBaseValues != null
                                ? application.GradeBaseValues.ToList() : new List<BaseValue>();

                            finalGradeValue = basevalues.Count > 0
                                ? Convert.ToDecimal(basevalues.Where(x => x.Key == "final_grade").FirstOrDefault().Value)
                                : 0;

                            userEventList.Add(new UserEventItem
                            {
                                UserId = studentsIds[i],
                                EventId = events[k].Id,
                                EventName = events[k].Title,
                                FinalGrade = finalGradeValue,
                                Date = application != null ? application.EventDate : (DateTimeOffset?)null
                            });
                        }
                    }

                    return userEventList;
                }
                catch (Exception ex)
                {
                    var testei = i;
                    var testek = k;
                    var testeb = b;
                    return new List<UserEventItem>();
                }
            }

            private async Task<List<StudentItem>> GetStudents(ObjectId trackId)
            {
                var students = new List<StudentItem>();
                var collection = _db.Database.GetCollection<Student>("Users");

                var studentsQuery = await collection.FindAsync(
                    x => x.TracksInfo != null
                );


                var studentsList = await studentsQuery.ToListAsync();

                var trackStudents = studentsList.Where(s =>
                        s.TracksInfo != null &&
                        s.TracksInfo.Any(
                            y => y.Id == trackId
                        )
                    ).ToList();

                for (int i = 0; i < trackStudents.Count; i++)
                {
                    var student = new StudentItem() {
                        Name = trackStudents[i].Name,
                        ImageUrl = trackStudents[i].ImageUrl,
                        Id = trackStudents[i].Id,
                        CurrentProgress = 0,
                        ProgressList = new List<ProgressItem>()
                    };

                    students.Add(student);

                }

                return students;
            }


            private async Task<List<EventApplication>> GetApplications(
                List<ObjectId> studentIds, List<ObjectId> eventIds, CancellationToken token
            ) {
                var query = await _db.Database
                        .GetCollection<EventApplication>("EventApplications")
                        .FindAsync(x =>
                            studentIds.Contains(x.UserId) && eventIds.Contains(x.EventId),
                            cancellationToken: token
                        );

                return await query.ToListAsync(cancellationToken: token);
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
