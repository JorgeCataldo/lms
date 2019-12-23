using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UsersCareer
{
    public class UserCareer : Entity
    {
        public bool ProfessionalExperience { get; set; }
        public List<ProfessionalExperience> ProfessionalExperiences { get; set; }
        public List<College> Colleges { get; set; }
        public List<Reward> Rewards { get; set; }
        public List<PerkLanguage> Languages { get; set; }
        public List<Perk> Abilities { get; set; }
        public List<Certificate> Certificates { get; set; }
        public List<string> Skills { get; set; }
        public bool TravelAvailability { get; set; }
        public bool MovingAvailability { get; set; }
        public string ShortDateObjectives { get; set; }
        public string LongDateObjectives { get; set; }

        public static Result<UserCareer> Create(ObjectId userId) {
            return Result.Ok(
                new UserCareer() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId
                }
            );
        }
    }

    public class ProfessionalExperience
    {
        public string Title { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class College
    {
        public ObjectId InstituteId { get; set; }
        public string Title { get; set; }
        public string Campus { get; set; }
        public string Name { get; set; }
        public string AcademicDegree { get; set; }
        public string Status { get; set; }
        public int? CompletePeriod { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string CR { get; set; }
    }

    public class Reward
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public DateTimeOffset? Date { get; set; }
    }

    public class Certificate
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

    public class Perk
    {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class PerkLanguage
    {
        public string Names { get; set; }
        public string Level { get; set; }
        public string Languages { get; set; }
    }
}