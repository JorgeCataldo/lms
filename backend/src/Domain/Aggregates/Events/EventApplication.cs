using System;
using System.Collections.Generic;
using Domain.SeedWork;
using MongoDB.Bson;

namespace Domain.Aggregates.Events
{
    public class EventApplication: Entity, IAggregateRoot
    {
        public ObjectId UserId { get; set; }
        public ObjectId EventId { get; set; }
        public ObjectId ScheduleId { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public PrepQuiz PrepQuiz { get; set; }
        public List<string> PrepQuizAnswers { get; set; }
        public List<PrepQuizAnswer> PrepQuizAnswersList { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTimeOffset RequestedDate { get; set; }
        public DateTimeOffset? ResolutionDate { get; set; }
        public decimal? OrganicGrade { get; set; }
        public decimal? InorganicGrade { get; set; }
        public bool? UserPresence { get; set; }
        public decimal? ForumGrade { get; set; }
        public List<BaseValue> GradeBaseValues { get; set; }
        public List<CustomEventGradeValue> CustomEventGradeValues { get; set; }
        public string TranscribedParticipation { get; set; }
    }

    public enum ApplicationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Full = 3
    }

    public class BaseValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class CustomEventGradeValue
    {
        public string Key { get; set; }
        public decimal? Grade { get; set; }
    }
}
