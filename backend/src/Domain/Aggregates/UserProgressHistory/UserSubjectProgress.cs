using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
//ROLLBACK BDQ
namespace Domain.Aggregates.UserProgressHistory
{
    public class UserSubjectProgress : Entity
    {
        public ObjectId ModuleId { get; set; }
        public ObjectId SubjectId { get; set; }
        public ObjectId UserId { get; set; }
        public int Level { get; set; }
        public decimal Progress { get; set; }
        public List<UserQuestion> Questions { get; set; }
        public DateTimeOffset QuestionListDefinition { get; set; }
        public List<UserAnswer> Answers { get; set; }
        public int Points { get; set; }
        //public decimal PassPercentage { get; set; }

        private UserSubjectProgress(ObjectId moduleId, ObjectId subjectId, ObjectId userId,
            int level, decimal progress, List<UserQuestion> questions,
            List<UserAnswer> answers, int points)//, decimal passPercentage)
        {
            ModuleId = moduleId;
            SubjectId = subjectId;
            UserId = userId;
            Level = level;
            Progress = progress;
            Questions = questions;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            QuestionListDefinition = CreatedAt;
            Answers = answers;
            Points = points;
            //PassPercentage = passPercentage;
        }

        public static UserSubjectProgress Create(ObjectId moduleId, ObjectId subjectId, ObjectId userId)
        {
            return new UserSubjectProgress(moduleId, subjectId, userId, Levels.Level.GetAllLevels().Data.First().Id,
                0, new List<UserQuestion>(), new List<UserAnswer>(), 0);//, 1);
        }
    }

    public class UserQuestion
    {
        public ObjectId QuestionId { get; set; }
        public ObjectId CorrectAnswerId { get; set; }
        public bool HasAnsweredCorrectly { get; set; }
        public bool Answered { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public int AnsweredCount { get; set; }
    }

    public class UserAnswer
    {
        public ObjectId QuestionId { get; set; }
        public DateTimeOffset AnswerDate { get; set; }
        public bool CorrectAnswer { get; set; }
        public int AnswerPoints { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public ObjectId AnswerId { get; set; }
        public ObjectId CorrectAnswerId { get; set; }
        public int? TotalDbQuestionNumber { get; internal set; }
        public int? TotalConceptNumber { get; internal set; }
        public decimal? LevelPercent { get; internal set; }
        public int? TotalQuestionNumber { get; internal set; }
        public int? ModuleQuestionsLimit { get; internal set; }
        public int? MaxPoints { get; internal set; }
        public int? TotalAnswers { get; internal set; }
        public int? InitWindow { get; internal set; }
        public int? EndWindow { get; internal set; }
        public int? TotalAccountablePoints { get; internal set; }
        public bool? HasAnsweredAllLevelQuestionsCorrectly { get; internal set; }
        public int? TotalApplicablePoints { get; internal set; }
        public decimal? AbsoluteProgress { get; internal set; }
        public int? OriginalLevel { get; internal set; }
        public int? FinalLevel { get; internal set; }
        //public decimal? PassPercentage { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public string QuestionConcepts { get; internal set; }
        public string AnswerWrongConcepts { get; internal set; }
    }
}
