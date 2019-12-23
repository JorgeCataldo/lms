using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class EventReaction : Entity, IAggregateRoot
    {
        public ObjectId EventId { get; set; }
        public ObjectId EventScheduleId { get; set; }
        public ReactionRating Didactic { get; set; }
        public ReactionRating ClassroomContent { get; set; }
        public ReactionRating StudyContent { get; set; }
        public ReactionRating TheoryAndPractice { get; set; }
        public ReactionRating UsedResources { get; set; }
        public ReactionRating EvaluationFormat { get; set; }
        public ReactionExpectation Expectation { get; set; }
        public string Suggestions { get; set; }
        public bool Approved { get; set; }

        public static Result<EventReaction> Create(
            ObjectId eventId, ObjectId scheduleId, ObjectId userId,
            ReactionRating didactic, ReactionRating classroomContent,
            ReactionRating studyContent, ReactionRating theoryAndPractice,
            ReactionRating usedResources, ReactionRating evalutionFormat,
            ReactionExpectation expectation, string suggestions
        )
        {
            return Result.Ok(
                new EventReaction()
                {
                    Id = ObjectId.GenerateNewId(),
                    EventId = eventId,
                    EventScheduleId = scheduleId,
                    CreatedBy = userId,
                    Didactic = didactic,
                    ClassroomContent = classroomContent,
                    StudyContent = studyContent,
                    TheoryAndPractice = theoryAndPractice,
                    UsedResources = usedResources,
                    EvaluationFormat = evalutionFormat,
                    Expectation = expectation,
                    Suggestions = suggestions,
                    Approved = false
                }
            );
        }

        public enum ReactionRating
        {
            Bad = 0,
            Unsatisfactory = 1,
            Satisfactory = 2,
            Good = 3,
            Excelent = 4
        }

        public enum ReactionExpectation
        {
            BelowExpectation = 0,
            AsExpected = 1,
            ExceedExpectation = 2
        }
    }
}
