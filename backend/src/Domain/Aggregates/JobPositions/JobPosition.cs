using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.JobPosition
{
    public class JobPosition : Entity
    {
        public string Title { get; set; }
        public DateTimeOffset DueTo { get; set; }
        public JobPositionPriorityEnum Priority { get; set; }
        public JobPositionStatusEnum Status { get; set; }
        public Employment Employment { get; set; }


        public static Result<JobPosition> Create(
            ObjectId recruiterId, string title,
            DateTimeOffset dueTo, JobPositionPriorityEnum priority,
            Employment employment
        ) {
            return Result.Ok(
                new JobPosition() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = recruiterId,
                    Title = title,
                    DueTo = dueTo,
                    Priority = priority,
                    Status = JobPositionStatusEnum.Open,
                    Employment = employment
                }
            );
        }
    }

    public enum JobPositionPriorityEnum
    {
        High = 1,
        Medium = 2,
        Low = 3
    }

    public enum JobPositionStatusEnum
    {
        Open = 1,
        Closed = 2
    }

    public class Employment
    {
        public Record Record { get; set; }
        public Activities Activities { get; set; }
        public PreRequirements PreRequirements { get; set; }
        public List<string> Values { get; set; }
        public Benefits Benefits { get; set; }
    }

    public class Record
    {
        public string Function { get; set; }
        public string ContractType { get; set; }
    }

    public class Activities
    {
        public string Activity { get; set; }
        public string Character { get; set; }
        public string Abilities { get; set; }
        public string Report { get; set; }
    }

    public class PreRequirements
    {
        public string Education { get; set; }
        public string CurseName { get; set; }
        public string DateConclusion { get; set; }
        public string MinTime { get; set; }
        public string CrAcumulation { get; set; }
        public List<ComplementaryInfo> ComplementaryInfo { get; set; }
        public string Certification { get; set; }
        public List<LanguageInfo> LanguageInfo { get; set; }
        public List<Others> Others { get; set; }
    }
    public class Others
    {
        public string Name { get; set; }
        public string Level { get; set; }
    }
    public class LanguageInfo
    {
        public string Language { get; set; }
        public string Level { get; set; }
    }
    public class ComplementaryInfo
    {
        public string Name { get; set; }
        public bool? Done { get; set; }
        public string Level { get; set; }
    }

    public class Benefits
    {
        public string Salary { get; set; }
        public List<ComplementaryBenefits> ComplementaryBenefits { get; set; }
        public string EmploymentType { get; set; }
        public string CrAcumulation { get; set; }
        public List<ComplementaryInfo> ComplementaryInfo { get; set; }
        public string Others { get; set; }
        public string Certification { get; set; }
        public string Language { get; set; }
        public string EmploymentChangeHouse { get; set; }
    }
    public class ComplementaryBenefits
    {
        public string Name { get; set; }
        public bool? Done { get; set; }
        public string Level { get; set; }
    }
}