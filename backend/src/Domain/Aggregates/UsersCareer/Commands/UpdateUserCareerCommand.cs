using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UsersCareer.Commands
{
    public class UpdateUserCareerCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string UserId { get; set; }
            public string CurrentUserRole { get; set; }
            public string CurrentUserId { get; set; }
            public UserCareerItem Career { get; set; }
        }

        public class UserCareerItem
        {
            public bool ProfessionalExperience { get; set; }
            public List<ProfessionalExperienceItem> ProfessionalExperiences { get; set; }
            public SchoolItem School { get; set; }
            public List<CollegeItem> Colleges { get; set; }
            public List<RewardItem> Rewards { get; set; }
            public List<CertificateItem> Certificates { get; set; }
            public List<PerkItem> Languages { get; set; }
            public List<PerkItem> Abilities { get; set; }
            public List<PerkItem> FixedAbilities { get; set; }
            public List<PerkLanguage> FixedLanguages { get; set; }
            public List<string> Skills { get; set; }
            public bool TravelAvailability { get; set; }
            public string ShortDateObjectives { get; set; }
            public string LongDateObjectives { get; set; }
        }

        public class ProfessionalExperienceItem
        {
            public string Title { get; set; }
            public string Role { get; set; }
            public string Description { get; set; }
            public DateTimeOffset? StartDate { get; set; }
            public DateTimeOffset? EndDate { get; set; }
        }

        public class SchoolItem
        {
            public string InstituteId { get; set; }
            public string Title { get; set; }
            public DateTimeOffset? StartDate { get; set; }
            public DateTimeOffset? EndDate { get; set; }
            public string CR { get; set; }
        }

        public class CollegeItem
        {
            public string InstituteId { get; set; }
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

        public class RewardItem
        {
            public string Title { get; set; }
            public string Name { get; set; }
            public string Link { get; set; }
            public DateTimeOffset? Date { get; set; }
        }

        public class CertificateItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
        }

        public class PerkItem
        {
            public string Name { get; set; }
            public bool? HasLevel { get; set; }
            public string Level { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole != "Secretary" && request.CurrentUserRole != "Admin" && request.CurrentUserRole != "Student")
                    return Result.Fail<Result>("Acesso Negado");

                if (request.CurrentUserRole == "Student" && request.CurrentUserId != request.UserId)
                    return Result.Fail<Result>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);
                
                var userCareer = await _db.UserCareerCollection.AsQueryable()
                    .Where(x => x.CreatedBy == userId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (userCareer == null)
                {
                    userCareer = UserCareer.Create(userId).Data;

                    await _db.UserCareerCollection.InsertOneAsync(
                        userCareer, cancellationToken: cancellationToken
                    );
                }

                userCareer = TransformToUserCareer(userCareer, request.Career);

                await _db.UserCareerCollection.ReplaceOneAsync(
                    t => t.CreatedBy == userCareer.CreatedBy, userCareer,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }

            private UserCareer TransformToUserCareer(UserCareer dbCareer, UserCareerItem career)
            {
                dbCareer.ProfessionalExperience = career.ProfessionalExperience;
                dbCareer.ProfessionalExperiences = new List<ProfessionalExperience>();
                dbCareer.TravelAvailability = career.TravelAvailability;
                dbCareer.ShortDateObjectives = career.ShortDateObjectives;
                dbCareer.LongDateObjectives = career.LongDateObjectives;
                
                dbCareer.Colleges = new List<College>();
                dbCareer.Rewards = new List<Reward>();
                dbCareer.Languages = new List<PerkLanguage>();
                dbCareer.Abilities = new List<Perk>();
                dbCareer.Certificates = new List<Certificate>();
                dbCareer.Skills = career.Skills;

                foreach (var professionalExperience in career.ProfessionalExperiences)
                {
                    dbCareer.ProfessionalExperiences.Add(new ProfessionalExperience
                    {
                        Title = professionalExperience.Title,
                        Role = professionalExperience.Role,
                        Description = professionalExperience.Description,
                        StartDate = professionalExperience.StartDate,
                        EndDate = professionalExperience.EndDate
                    });
                }

                foreach (var college in career.Colleges)
                {
                    var instituteId = string.IsNullOrEmpty(college.InstituteId) ?
                        ObjectId.Empty : ObjectId.Parse(college.InstituteId);

                    dbCareer.Colleges.Add(new College
                    {
                        InstituteId = instituteId,
                        Title = college.Title,
                        Campus = college.Campus,
                        Name = college.Name,
                        AcademicDegree = college.AcademicDegree,
                        Status = college.Status,
                        CompletePeriod = college.CompletePeriod,
                        StartDate = college.StartDate,
                        EndDate = college.EndDate,
                        CR = college.CR
                    });
                }

                foreach (var reward in career.Rewards)
                {
                    dbCareer.Rewards.Add(new Reward
                    {
                        Title = reward.Title,
                        Name = reward.Name,
                        Link = reward.Link,
                        Date = reward.Date
                    });
                }

                foreach (var certificate in career.Certificates)
                {
                    dbCareer.Certificates.Add(new Certificate
                    {
                        Title = certificate.Title,
                        Link = certificate.Link
                    });
                }

                foreach (var languages in career.FixedLanguages)
                {
                    if (!string.IsNullOrEmpty(languages.Languages) && !string.IsNullOrEmpty(languages.Level))
                    {
                        dbCareer.Languages.Add(new PerkLanguage
                        {
                            Names = languages.Names,
                            Level = languages.Level,
                            Languages = languages.Languages
                        });
                    }
                }

                foreach (var ability in career.FixedAbilities)
                {
                    if (ability.HasLevel.HasValue && ability.HasLevel.Value)
                    {
                        dbCareer.Abilities.Add(new Perk
                        {
                            Name = ability.Name,
                            Level = ability.Level
                        });
                    }
                }

                    foreach (var ability in career.Abilities)
                {
                    dbCareer.Abilities.Add(new Perk
                    {
                        Name = ability.Name,
                        Level = ability.Level
                    });
                }

                return dbCareer;
            }
        }
    }
}
