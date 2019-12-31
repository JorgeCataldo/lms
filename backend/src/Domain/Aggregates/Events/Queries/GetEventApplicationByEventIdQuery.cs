using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Queries
{
    public class GetEventApplicationByEventIdQuery
    {
        public class Contract : CommandContract<Result<EventApplicationItem>>
        {
            public string EventId { get; set; }
            public string ScheduleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class EventApplicationItem
        {
            public Event Event { get; set; }
            public List<UserEventItem> Applications { get; set; }
        }

        public class UserPreview
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string LineManager { get; set; }
            public bool IsBlocked { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public User.RelationalItem Rank { get; set; }
        }

        public class UserEventItem
        {
            public ObjectId Id { get; set; }
            public ObjectId UserId { get; set; }
            public UserPreview User { get; set; }
            public ObjectId EventId { get; set; }
            public PrepQuiz PrepQuiz { get; set; }
            public List<string> PrepQuizAnswers { get; set; }
            public List<PrepQuizAnswer> PrepQuizAnswersList { get; set; }
            public ApplicationStatus ApplicationStatus { get; set; }
            public DateTimeOffset RequestedDate { get; set; }
            public DateTimeOffset? ResolutionDate { get; set; }
            public decimal? OrganicGrade { get; set; }
            public decimal? InorganicGrade { get; set; }
            public decimal? ForumGrade { get; set; }
            public bool FilledEventReaction { get; set; }
            public bool? UserPresence { get; set; }
            public bool EventRequirement { get; set; }
            public List<CustomEventGradeValue> CustomEventGradeValues { get; set; }
            public string TranscribedParticipation { get; set; }
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public int StudentsCount { get; set; }
            public decimal Duration { get; set; }
        }

        public class StudentItem
        {
            public ObjectId Id { get; set; }
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
        }

        public class TrackModuleItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int Order { get; set; }
        }

        public class TrackEventItem
        {
            public ObjectId EventId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public List<ObjectId> LateStudents { get; set; } = new List<ObjectId>();
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<EventApplicationItem>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<EventApplicationItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                string appEventGradeKeys = _configuration[$"Permissions:EventGradeKeys"];
                var eventGradeKeys = new List<string>();
                if (!string.IsNullOrEmpty(appEventGradeKeys))
                {
                    eventGradeKeys = appEventGradeKeys.Split(',').ToList();
                }
                var subordinatesUsersIds = new List<ObjectId>();
                var isTutorOrInstructor = false;
                var evtId = ObjectId.Parse(request.EventId);
                var SchId = ObjectId.Parse(request.ScheduleId);
                var eveApp = new EventApplicationItem();
                var userId = ObjectId.Parse(request.UserId);

                if (request.UserRole == "Student")
                {
                    var instructorTutor = await _db.EventCollection.AsQueryable()
                        .FirstOrDefaultAsync(x => 
                        x.Id == evtId && 
                        x.InstructorId == userId || 
                        x.TutorsIds.Contains(userId)
                    );

                    if (instructorTutor != null)
                    {
                        isTutorOrInstructor = true;
                    }
                    else
                    {
                        var responsible = await _db
                            .Database
                            .GetCollection<Responsible>("Responsibles")
                            .AsQueryable()
                            .FirstOrDefaultAsync(x => x.ResponsibleUserId == userId);

                        if (responsible == null)
                            return Result.Fail<EventApplicationItem>("Acesso Negado");

                        subordinatesUsersIds = responsible.SubordinatesUsersIds;
                    }
                }

                var eventDb = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => x.Id == evtId)
                    .FirstOrDefaultAsync();

                var eventList = new List<UserEventItem>();

                if (request.UserRole == "Student" && !isTutorOrInstructor)
                {
                    eventList = await _db.EventApplicationCollection
                   .AsQueryable()
                   .Where(x => x.EventId == evtId &&
                       x.ScheduleId == SchId &&
                       subordinatesUsersIds.Contains(x.UserId))
                   .Select(x => new UserEventItem()
                   {
                       Id = x.Id,
                       EventId = x.EventId,
                       UserId = x.UserId,
                       ApplicationStatus = x.ApplicationStatus,
                       PrepQuizAnswers = x.PrepQuizAnswers,
                       PrepQuizAnswersList = x.PrepQuizAnswersList,
                       PrepQuiz = x.PrepQuiz,
                       RequestedDate = x.RequestedDate,
                       ResolutionDate = x.ResolutionDate,
                       OrganicGrade = x.OrganicGrade == 0 ? null : x.OrganicGrade,
                       InorganicGrade = x.InorganicGrade == 0 ? null : x.InorganicGrade,
                       ForumGrade = x.ForumGrade == 0 ? null : x.ForumGrade,
                       UserPresence = x.UserPresence,
                       CustomEventGradeValues = x.CustomEventGradeValues ?? new List<CustomEventGradeValue>(),
                        TranscribedParticipation = x.TranscribedParticipation
                   }).ToListAsync();
                }
                else
                {
                    eventList = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => x.EventId == evtId &&
                        x.ScheduleId == SchId)
                    .Select(x => new UserEventItem()
                    {
                        Id = x.Id,
                        EventId = x.EventId,
                        UserId = x.UserId,
                        ApplicationStatus = x.ApplicationStatus,
                        PrepQuizAnswers = x.PrepQuizAnswers,
                        PrepQuizAnswersList = x.PrepQuizAnswersList,
                        PrepQuiz = x.PrepQuiz,
                        RequestedDate = x.RequestedDate,
                        ResolutionDate = x.ResolutionDate,
                        OrganicGrade = x.OrganicGrade == 0 ? null : x.OrganicGrade,
                        InorganicGrade = x.InorganicGrade == 0 ? null : x.InorganicGrade,
                        ForumGrade = x.ForumGrade == 0 ? null : x.ForumGrade,
                        UserPresence = x.UserPresence,
                        CustomEventGradeValues = x.CustomEventGradeValues ?? new List<CustomEventGradeValue>(),
                        TranscribedParticipation = x.TranscribedParticipation
                    }).ToListAsync();
                }

                if (eventList.Count > 0)
                {
                    var userList = await _db.UserCollection
                    .AsQueryable()
                    .Select(x => new UserPreview() {
                        Id = x.Id,
                        ImageUrl = x.ImageUrl,
                        Name = x.Name,
                        Email = x.Email,
                        LineManager = x.LineManager,
                        IsBlocked = x.IsBlocked,
                        CreatedAt = x.CreatedAt,
                        Rank = x.Rank
                    }).ToListAsync();

                    var userIds = userList.Select(x => x.Id).ToArray();
                    var reactionList = await _db.EventReactionCollection
                        .AsQueryable()
                        .Where(x =>
                            x.EventScheduleId == SchId &&
                            userIds.Contains(x.CreatedBy)
                        )
                        .Select(x=>x.CreatedBy)
                        .ToListAsync();

                    foreach (UserEventItem eventItem in eventList)
                    {
                        var listGradeEvents = new List<CustomEventGradeValue>();
                        foreach (string key in eventGradeKeys)
                        {
                            listGradeEvents.Add(new CustomEventGradeValue
                            {
                                Key = key
                            });
                        }
                        var userItem = userList.Find(x => x.Id == eventItem.UserId);
                        eventItem.User = userItem;
                        eventItem.FilledEventReaction = reactionList.Contains(eventItem.UserId);
                        eventItem.EventRequirement = true;
                        var baseGrades = eventItem.CustomEventGradeValues.Where(x => eventGradeKeys.Contains(x.Key)).ToList();
                        foreach (CustomEventGradeValue baseGrade in baseGrades)
                        {
                            var listGrade = listGradeEvents.FirstOrDefault(x => x.Key == baseGrade.Key);
                            if (listGrade != null)
                            {
                                listGrade.Grade = baseGrade.Grade;
                            }
                        }
                        eventItem.CustomEventGradeValues = listGradeEvents;
                    }
                }
                    
                eveApp.Event = eventDb;
                eveApp.Applications = eventList;

                return Result.Ok(eveApp);
            }

            private async Task<bool> CheckFilledEventReaction(ObjectId scheduleId, ObjectId userId)
            {
                var query = await _db.Database
                    .GetCollection<EventReaction>("EventReactions")
                    .FindAsync(x =>
                        x.EventScheduleId == scheduleId &&
                        x.CreatedBy == userId
                    );

                return await query.AnyAsync();
            }
        }
    }
}
