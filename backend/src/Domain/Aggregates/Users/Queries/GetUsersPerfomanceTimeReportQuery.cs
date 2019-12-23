using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Data;
using Domain.Enumerations;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUsersPerfomanceTimeReportQuery
    {
        public class Contract : IRequest<Result<List<List<DetailItem>>>>
        {
            public string TrackIds { get; set; }
            public UserPerformanceTimeTypeEnum UserPerformanceTimeType { get; set; }
            public string UserRole { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public List<UserProgress> ModulesInfo { get; set; }
            public List<UserProgress> TracksInfo { get; set; }
            public List<UserProgress> EventsInfo { get; set; }
        }

        public class ActionItem
        {
            public ObjectId ActionId { get; set; }
            public string ModuleId { get; set; }
            public ObjectId UserId { get; set; }
            public DateTimeOffset Date { get; set; }
        }

        public class DetailItem
        {
            public string Name { get; set; }
            public string TrackName { get; set; }
            public int? FromStartDays { get; set; }
            public int? RemainingDays { get; set; }
            public List<EventDetailItem> EventDetailItems { get; set; }
            public List<ModuleDetailItem> ModuleDetailItems { get; set; }
            public decimal TrackProgress { get; set; }
            public decimal TrackExpectedProgress { get; set; }
            public decimal StandardDeviation { get; set; }
        }

        public class EventDetailItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public int? RemainingDays { get; set; }
        }

        public class ModuleDetailItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public int? FirstInteractionDays { get; set; }
            public int? BdqFirstInteractionDays { get; set; }
            public int? RemainingDays { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<List<DetailItem>>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<List<DetailItem>>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.UserRole != "Admin")
                        return Result.Fail<List<List<DetailItem>>>("Acesso Negado");

                    var trackIdList = request.TrackIds.Split(",").ToList();

                    List<UserItem> students = new List<UserItem>();
                    List<Track> tracks = new List<Track>();
                    List<Event> events = new List<Event>();
                    List<Module> modules = new List<Module>();
                    List<List<DetailItem>> details = new List<List<DetailItem>>();

                    var studentIds = new List<ObjectId>();
                    var trackIds = new List<ObjectId>();
                    var eventIds = new List<ObjectId>();
                    var moduleIds = new List<ObjectId>();

                    if (trackIdList != null && trackIdList.Count > 0)
                    {

                        for (int i = 0; i < trackIdList.Count; i++)
                        {
                            trackIds.Add(ObjectId.Parse(trackIdList[i]));
                        }

                        tracks = _db.TrackCollection.AsQueryable()
                            .Where(x => trackIds.Contains(x.Id))
                            .ToList();
                    }
                    else
                    {
                        tracks = _db.TrackCollection.AsQueryable()
                            .ToList();
                    }

                    for (int i = 0; i < tracks.Count; i++)
                    {
                        var currentTrack = tracks[i];
                        if (currentTrack.ModulesConfiguration != null && currentTrack.ModulesConfiguration.Count > 0)
                        {
                            moduleIds.AddRange(currentTrack.ModulesConfiguration.Select(x => x.ModuleId));
                        }
                        if (currentTrack.EventsConfiguration != null && currentTrack.EventsConfiguration.Count > 0)
                        {
                            eventIds.AddRange(currentTrack.EventsConfiguration.Select(x => x.EventId));
                        }
                    }

                    if (eventIds.Count > 0)
                    {
                        eventIds = eventIds.Distinct().ToList();
                        events = _db.EventCollection.AsQueryable()
                             .Where(x => eventIds.Contains(x.Id))
                             .ToList();
                    }

                    if (moduleIds.Count > 0)
                    {
                        moduleIds = moduleIds.Distinct().ToList();
                        modules = _db.ModuleCollection.AsQueryable()
                             .Where(x => moduleIds.Contains(x.Id))
                             .ToList();
                    }

                    students = _db.UserCollection.AsQueryable()
                        .Where(x => x.TracksInfo.Any(y => trackIds.Contains(y.Id) /*&& y.CreatedAt != null*/))
                        .Select(x => new UserItem()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            TracksInfo = x.TracksInfo,
                            EventsInfo = x.EventsInfo,
                            ModulesInfo = x.ModulesInfo
                        })
                        .ToList();

                    if (students == null || students.Count == 0)
                        return Result.Fail<List<List<DetailItem>>>("Alunos não encontrados");

                    studentIds = students.Select(x => x.Id).ToList();

                    var trackProgress = _db.UserTrackProgressCollection.AsQueryable()
                        .Where(x => studentIds.Contains(x.UserId) && trackIds.Contains(x.TrackId))
                        .ToList();

                    var eventApplications = _db.EventApplicationCollection.AsQueryable()
                        .Where(x => studentIds.Contains(x.UserId) && eventIds.Contains(x.EventId))
                        .ToList();

                    var moduleStringIds = moduleIds.Select(x => x.ToString()).ToList();

                    var examActions = _db.ActionCollection.AsQueryable()
                        .Where(x =>
                            studentIds.Contains(x.CreatedBy) &&
                            moduleStringIds.Contains(x.ModuleId) &&
                            x.Description == "exam-start" &&
                            x.Type == ActionType.Access
                        )
                        .OrderBy(x => x.CreatedAt)
                        .GroupBy(x => new { x.CreatedBy, x.ModuleId })
                        .Select(x =>
                            new ActionItem
                            {
                                ActionId = x.First().Id,
                                ModuleId = x.First().ModuleId,
                                UserId = x.First().CreatedBy,
                                Date = x.First().CreatedAt
                            }
                        )
                        .ToList();

                    var moduleActions = _db.ActionCollection.AsQueryable()
                        .Where(x =>
                            studentIds.Contains(x.CreatedBy) &&
                            moduleStringIds.Contains(x.ModuleId) &&
                            x.Description == "content-access" &&
                            x.Type == ActionType.Access
                        )
                        .OrderBy(x => x.CreatedAt)
                        .GroupBy(x => new { x.CreatedBy, x.ModuleId })
                        .Select(x =>
                            new ActionItem
                            {
                                ActionId = x.First().Id,
                                ModuleId = x.First().ModuleId,
                                UserId = x.First().CreatedBy,
                                Date = x.First().CreatedAt
                            }
                        )
                        .ToList();

                    var usermoduleProgresses = _db.UserModuleProgressCollection.AsQueryable()
                        .Where(x => moduleIds.Contains(x.ModuleId) && studentIds.Contains(x.UserId))
                        .ToList();

                    var today = DateTimeOffset.Now;
                    for (int i = 0; i < tracks.Count; i++)
                    {
                        var currentTrack = tracks[i];
                        var currentTrackDetail = new List<DetailItem>();
                        for (int j = 0; j < students.Count; j++)
                        {
                            var currentStudent = students[j];
                            var currentDetail = new DetailItem
                            {
                                Name = currentStudent.Name,
                                TrackName = currentTrack.Title,
                                EventDetailItems = new List<EventDetailItem>(),
                                ModuleDetailItems = new List<ModuleDetailItem>()
                            };


                            var currentStudentTrackProgress = trackProgress.FirstOrDefault(x => x.UserId == currentStudent.Id);
                            if (currentStudentTrackProgress != null)
                            {
                                currentDetail.TrackProgress = currentStudentTrackProgress.Progress;
                            }
                            else
                            {
                                currentDetail.TrackProgress = 0;
                            }

                            var trackConfig = currentStudent.TracksInfo.FirstOrDefault(x => x.Id == currentTrack.Id);

                            if(trackConfig == null)
                            {
                                continue;
                            }

                            if (trackConfig.CreatedAt != null && trackConfig.CreatedAt != null && today > trackConfig.CreatedAt)
                            {
                                currentDetail.FromStartDays = (today - trackConfig.CreatedAt.Value).Days;
                            }
                            else
                            {
                                currentDetail.FromStartDays = null;
                            }

                            if (currentTrack.ValidFor != null && trackConfig.CreatedAt != null)
                            {
                                var remainingDateTime = trackConfig.CreatedAt.Value.AddDays(currentTrack.ValidFor.Value);
                                if (today > remainingDateTime)
                                {
                                    currentDetail.RemainingDays = null;
                                    currentDetail.TrackExpectedProgress = 1;
                                }
                                else
                                {
                                    currentDetail.RemainingDays = currentTrack.ValidFor - (remainingDateTime - today).Days;
                                    currentDetail.TrackExpectedProgress = (decimal)currentDetail.RemainingDays.Value / 100;
                                }
                            }
                            else
                            {
                                currentDetail.RemainingDays = null;
                                currentDetail.TrackExpectedProgress = 1;
                            }

                            currentDetail.StandardDeviation = currentDetail.TrackProgress - currentDetail.TrackExpectedProgress;

                            for (int k = 0; k < currentTrack.EventsConfiguration.Count; k++)
                            {
                                var currentEventConfiguration = currentTrack.EventsConfiguration[k];
                                var eventInfo = events.First(x => x.Id == currentEventConfiguration.EventId);
                                var currentEventDetail = new EventDetailItem
                                {
                                    Id = eventInfo.Id,
                                    Name = eventInfo.Title
                                };

                                if (currentEventConfiguration.CutOffDate != null)
                                {
                                    currentEventDetail.RemainingDays = (currentEventConfiguration.CutOffDate.Value - today).Days;
                                }
                                else
                                {
                                    var eventInfoSchedule = eventInfo.Schedules.First(x => x.Id == currentEventConfiguration.EventScheduleId);
                                    currentEventDetail.RemainingDays = (eventInfoSchedule.EventDate - today).Days;
                                }
                                currentDetail.EventDetailItems.Add(currentEventDetail);
                            }

                            for (int l = 0; l < currentTrack.ModulesConfiguration.Count; l++)
                            {
                                var currentModuleConfiguration = currentTrack.ModulesConfiguration[l];
                                var moduleInfo = modules.First(x => x.Id == currentModuleConfiguration.ModuleId);
                                var moduleUserExamAction = examActions
                                    .FirstOrDefault(x => x.ModuleId == moduleInfo.Id.ToString() && x.UserId == currentStudent.Id);
                                var moduleUserModuleAction = moduleActions
                                    .FirstOrDefault(x => x.ModuleId == moduleInfo.Id.ToString() && x.UserId == currentStudent.Id);

                                var currentModuleDetail = new ModuleDetailItem
                                {
                                    Id = moduleInfo.Id,
                                    Name = moduleInfo.Title
                                };

                                if (currentModuleConfiguration.CutOffDate != null)
                                {
                                    currentModuleDetail.RemainingDays = (currentModuleConfiguration.CutOffDate.Value - today).Days;
                                }
                                else
                                {
                                    currentModuleDetail.RemainingDays = null;
                                }

                                if (moduleUserExamAction != null)
                                {
                                    currentModuleDetail.BdqFirstInteractionDays = (today - moduleUserExamAction.Date).Days;
                                }
                                else
                                {
                                    currentModuleDetail.BdqFirstInteractionDays = null;
                                }

                                if (moduleUserModuleAction != null)
                                {
                                    currentModuleDetail.FirstInteractionDays = (today - moduleUserModuleAction.Date).Days;
                                }
                                else
                                {
                                    currentModuleDetail.FirstInteractionDays = null;
                                }
                                currentDetail.ModuleDetailItems.Add(currentModuleDetail);
                            }
                            currentTrackDetail.Add(currentDetail);
                        }
                        details.Add(currentTrackDetail);
                    }

                    return Result.Ok(details);
                }
                catch(Exception ex)
                {
                    return Result.Fail<List<List<DetailItem>>>("Acesso Negado");
                }
            }
        }
    }
}
