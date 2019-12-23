using System;
using System.Collections.Generic;
using Domain.SeedWork;
using Tg4.Infrastructure.Functional;
using ValueObject = Domain.SeedWork.ValueObject;

namespace Domain.ValueObjects
{
    public class UserProgress: ValueObject
    {
        public ProgressType ProgressType { get; set; }
        public int Level { get; set; }
        public decimal Percentage { get; set; }

        
        public UserProgress(ProgressType progressType, int level, decimal percentage)
        {
            ProgressType = progressType;
            Level = level;
            Percentage = percentage;
        }

        public static Result<UserProgress> Create(ProgressType progressType, int level, decimal percentage)
        {
            if (percentage < 0 || percentage > 1)
                return Result.Fail<UserProgress>("Percentual deve ser maior que zero e menor ou igual a 1");

            return Result.Ok(new UserProgress(progressType, level, percentage));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ProgressType;
            yield return Level;
            yield return Percentage;
        }
    }
    
    public enum ProgressType
    {
        ModuleProgress, EventProgress, TrackProgress, SubjectProgress
    }
}