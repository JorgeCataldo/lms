using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Aggregates.Levels;
using Domain.Aggregates.ModulesDrafts;
using Domain.Aggregates.UserProgressHistory;
using Domain.Data;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules
{
    public class Module : Entity
    {
        public bool Published { get; set; }
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public ObjectId? InstructorId { get; set; }
        public List<ObjectId> ExtraInstructorIds { get; set; }
        public List<ObjectId> TutorsIds { get; set; }
        public string Instructor { get; set; }
        public string InstructorMiniBio { get; set; }
        public string InstructorImageUrl { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public Duration VideoDuration { get; set; }
        public int? ValidFor { get; set; }
        public string StoreUrl { get; set; }
        public string EcommerceUrl { get; set; }
        public long? EcommerceId { get; set; }
        public int? QuestionsLimit { get; set; }
        public Duration Duration { get; set; }
        public string CertificateUrl { get; set; }
        public ModuleGradeTypeEnum ModuleGradeType { get; set; }

        public List<ValueObjects.Tag> Tags { get; set; }
        public List<EcommerceModule> EcommerceProducts { get; set; }
        public List<ObjectId> AssistantProfessorIds { get; set; }
        public List<SupportMaterial> SupportMaterials { get; set; }
        public List<Requirement> Requirements { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<ManageModuleWeights> ModuleWeights { get; set; }

        private Module() : base()
        {
            SupportMaterials = new List<SupportMaterial>();
            AssistantProfessorIds = new List<ObjectId>();
            Requirements = new List<Requirement>();
            Tags = new List<ValueObjects.Tag>();
            Subjects = new List<Subject>();
        }

        public static Result<Module> Create(string title, string excerpt, string instructor, string imageUrl,
            List<ObjectId> assistantProfessorIds, ObjectId? instructorId, List<SupportMaterial> supportMaterials,
            List<Requirement> requirements, List<ValueObjects.Tag> tags, bool published, Duration duration, List<Subject> subjects,
            string certificateUrl, List<ObjectId> tutorsIds, List<ObjectId> extraInstructorIds, string storeUrl = "", string ecommerceUrl = "",
            ModuleGradeTypeEnum moduleGradeType = ModuleGradeTypeEnum.SubjectsLevel, int? validFor = null)
        {
            if (title.Length > 200)
                return Result.Fail<Module>($"Tamanho máximo do título do módulo é de 200 caracteres. ({title})");

            var module = new Module()
            {
                Id = ObjectId.GenerateNewId(),
                Title = title,
                Excerpt = excerpt,
                Instructor = instructor,
                ImageUrl = imageUrl,
                AssistantProfessorIds = assistantProfessorIds,
                InstructorId = instructorId,
                SupportMaterials = supportMaterials,
                Requirements = requirements,
                Tags = tags,
                Published = published,
                Duration = duration,
                Subjects = subjects,
                CertificateUrl = certificateUrl,
                TutorsIds = tutorsIds,
                ExtraInstructorIds = extraInstructorIds,
                StoreUrl = storeUrl,
                EcommerceUrl = ecommerceUrl,
                ModuleGradeType = moduleGradeType,
                ValidFor = validFor
            };

            return Result.Ok(module);
        }

        public static Result<Module> Create(string title, string excerpt, string instructorId, string instructor, string instructorMiniBio,
            string imageUrl, string instructorImageUrl, bool published, Duration duration, List<ValueObjects.Tag> tags = null,
            string certificateUrl = "", List<ObjectId> tutorsIds = null, List<ObjectId> extraInstructorIds = null,
            string storeUrl = "", string ecommerceUrl = "", ModuleGradeTypeEnum moduleGradeType = ModuleGradeTypeEnum.SubjectsLevel, int? validFor = null)
        {
            var module = new Module()
            {
                Id = ObjectId.GenerateNewId(),
                Title = title,
                Excerpt = excerpt,
                InstructorId = string.IsNullOrEmpty(instructorId) ? ObjectId.Empty : ObjectId.Parse(instructorId),
                Instructor = instructor,
                InstructorMiniBio = instructorMiniBio,
                ImageUrl = imageUrl,
                InstructorImageUrl = instructorImageUrl,
                Published = published,
                Duration = duration,
                Tags = tags,
                CertificateUrl = certificateUrl,
                TutorsIds = tutorsIds,
                ExtraInstructorIds = extraInstructorIds,
                StoreUrl = storeUrl,
                EcommerceUrl = ecommerceUrl,
                ModuleGradeType = moduleGradeType,
                ValidFor = validFor
            };

            return Result.Ok(module);
        }

        public static Result<bool> IsInstructor(Module module, ObjectId userId)
        {
            if (module.InstructorId.HasValue || module.ExtraInstructorIds.Any())
            {
                if (module.InstructorId.Value == userId)
                {
                    return Result.Ok(true);
                }
                else if (module.ExtraInstructorIds.Any(x => x == userId))
                {
                    return Result.Ok(true);
                }
            }
            return Result.Ok(false);
        }

        public static Result<List<ModuleGradeItem>> GetModulesGrades(IDbContext _db, List<ObjectId> userIds, List<ObjectId> moduleIds, DateTimeOffset? cutOffDate = null)
        {
            var moduleGradeList = new List<ModuleGradeItem>();

            try
            {
                if (cutOffDate == null)
                {
                    cutOffDate = DateTimeOffset.MaxValue;
                }

                var modules = _db.ModuleCollection
                   .AsQueryable()
                   .Where(x =>
                       moduleIds.Contains(x.Id)
                   )
                   .ToList();

                var modulesProgress = _db.UserModuleProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        userIds.Contains(x.UserId) &&
                        moduleIds.Contains(x.ModuleId)
                    )
                    .ToList();

                var subjectProgress = _db.UserSubjectProgressCollection
                    .AsQueryable()
                    .Where(x =>
                        userIds.Contains(x.UserId) &&
                        moduleIds.Contains(x.ModuleId) &&
                        x.Answers.Any(a => a.AnswerDate <= cutOffDate)
                    )
                    .Select(x => new UserSubjectProgressItem
                    {
                        ModuleId = x.ModuleId,
                        UserId = x.UserId,
                        Level = x.Level,
                        Answers = x.Answers.Select(y => y.CorrectAnswer)
                    })
                    .ToList();

                var levelsCount = Level.GetAllLevels().Data.Count();

                foreach (UserModuleProgress prog in modulesProgress)
                {
                    var moduleGrade = moduleGradeList.FirstOrDefault(x => x.ModuleId == prog.ModuleId);
                    var module = modules.FirstOrDefault(x => x.Id == prog.ModuleId);
                    if (module != null)
                    {
                        if (moduleGrade == null)
                        {
                            var newModuleGrade = new ModuleGradeItem
                            {
                                ModuleId = module.Id,
                                ModuleName = module.Title,
                                UserGrades = new List<ModuleUserGradeItem>()
                            };

                            var newModuleUserGrade = new ModuleUserGradeItem
                            {
                                UserId = prog.UserId,
                                Grade = CalculateUserGrade(
                                    module,
                                    prog,
                                    subjectProgress.Where(x => x.ModuleId == prog.ModuleId && x.UserId == prog.UserId).ToList(),
                                    levelsCount
                                )
                            };
                            newModuleGrade.UserGrades.Add(newModuleUserGrade);

                            moduleGradeList.Add(newModuleGrade);
                        }
                        else
                        {
                            var newModuleUserGrade = new ModuleUserGradeItem
                            {
                                UserId = prog.UserId,
                                Grade = CalculateUserGrade(
                                    module,
                                    prog,
                                    subjectProgress.Where(x => x.ModuleId == prog.ModuleId && x.UserId == prog.UserId).ToList(),
                                    levelsCount
                                )
                            };
                            moduleGrade.UserGrades.Add(newModuleUserGrade);
                        }
                    }
                }


                return Result.Ok(moduleGradeList);
            }
            catch(Exception ex)
            {
                var teste = ex;
                return Result.Ok(moduleGradeList);
            }
        }

        private static decimal CalculateUserGrade(Module module, UserModuleProgress moduleProgress,
            List<UserSubjectProgressItem> userProgress, decimal levelsCount)
        {
            switch (module.ModuleGradeType)
            {
                case ModuleGradeTypeEnum.Percentage:
                    decimal badgeValue = 0;
                    if (moduleProgress == null)
                    {
                        return badgeValue;
                    }
                    switch(moduleProgress.Level)
                    {
                        case 1:
                            badgeValue = (decimal)0.3;
                            break;
                        case 2:
                            badgeValue = (decimal)0.6;
                            break;
                        case 3:
                            badgeValue = (decimal)0.9;
                            break;
                        case 4:
                            badgeValue = (decimal)1.0;
                            break;
                    }
                    var totalAnswers = new List<bool>();
                    foreach(List<bool> userAnswerList in userProgress.Select(x => x.Answers).ToList())
                    {
                        totalAnswers.AddRange(userAnswerList);
                    }
                    var correctAnswers = totalAnswers.Where(x => x).ToList();
                    if (correctAnswers.Count == 0 || totalAnswers.Count == 0 || badgeValue == 0)
                    {
                        return 0;
                    }
                    return ((decimal)correctAnswers.Count / (totalAnswers.Count > 0 ? totalAnswers.Count : 1)) * 
                        badgeValue;

                case ModuleGradeTypeEnum.SubjectsLevel:
                default:
                    if (userProgress == null || userProgress.Count == 0)
                    {
                        return 0;
                    }
                    var levelSum = userProgress.Select(x => x.Level).Sum();
                    if (levelSum == 0 || module.Subjects == null || module.Subjects.Count == 0 || levelsCount == 0)
                    {
                        return 0;
                    }
                    return levelSum / (module.Subjects.Count * levelsCount);
            }
        }
    }

    public class ModuleGradeItem {

        public ObjectId ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<ModuleUserGradeItem> UserGrades { get; set; }
    }

    public class ModuleUserGradeItem
    {
        public ObjectId UserId { get; set; }
        public decimal Grade { get; set; }
    }

    public enum ModuleGradeTypeEnum
    {
        SubjectsLevel = 1,
        Percentage = 2
    }

    public class UserSubjectProgressItem
    {
        public ObjectId ModuleId { get; set; }
        public ObjectId UserId { get; set; }
        public int Level { get; set; }
        public IEnumerable<bool> Answers { get; set; }
    }

    public class EcommerceModule
    {
        public long EcommerceId { get; set; }
        public int UsersAmount { get; set; }
        public bool DisableEcommerce { get; set; }
        public string Price { get; set; }
        public bool DisableFreeTrial { get; set; }
        public string LinkEcommerce { get; set; }
        public string LinkProduct { get; set; }
        public string Subject { get; set; }
        public string Hours { get; set; }

        public static Result<EcommerceModule> Create(
            long ecommerceId, int usersAmount, bool disableEcommerce, string price,
            bool disableFreeTrial, string linkEcommerce, string linkProduct,
            string subject, string hours
        )
        {
            return Result.Ok(
                new EcommerceModule
                {
                    EcommerceId = ecommerceId,
                    UsersAmount = usersAmount,
                    DisableEcommerce = disableEcommerce,
                    Price = price,
                    DisableFreeTrial = disableFreeTrial,
                    LinkEcommerce = linkEcommerce,
                    LinkProduct = linkProduct,
                    Subject = subject,
                    Hours = hours
                }
            );
        }
    }
}