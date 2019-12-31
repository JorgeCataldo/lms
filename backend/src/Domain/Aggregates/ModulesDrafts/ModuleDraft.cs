using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Aggregates.Modules;
using Domain.Extensions;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ModulesDrafts
{
    public class ModuleDraft : Entity
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId ModuleId { get; set; }
        public bool DraftPublished { get; set; }
        public DateTimeOffset DraftPlubishedAt { get; set; }

        public string Title { get; set; }
        public string Excerpt { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId? InstructorId { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public List<ObjectId> ExtraInstructorIds { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public List<ObjectId> TutorsIds { get; set; }
        public string Instructor { get; set; }
        public string InstructorMiniBio { get; set; }
        public string InstructorImageUrl { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public Duration VideoDuration { get; set; }
        public bool Published { get; set; }
        public Duration Duration { get; set; }
        public int? ValidFor { get; set; }

        public string CertificateUrl { get; set; }
        public string StoreUrl { get; set; }
        public string EcommerceUrl { get; set; }
        public int? QuestionsLimit { get; set; }
        public bool CreateInEcommerce { get; set; }
        public long? EcommerceId { get; set; }
        public ModuleGradeTypeEnum ModuleGradeType { get; set; }

        public List<Tag> Tags { get; set; }
        public List<EcommerceModuleDraft> EcommerceProducts { get; set; }
        public List<ManageModuleWeights> ModuleWeights { get; set; }
        public List<SupportMaterial> SupportMaterials { get; set; }
        public List<Requirement> Requirements { get; set; }
        public List<Subject> Subjects { get; set; }

        public static Result<ModuleDraft> Create(
            ObjectId moduleId, string title, string excerpt, string imageUrl, bool published,
            ObjectId? instructorId, string instructor, string instructorMiniBio, string instructorImageUrl,
            Duration duration, Duration videoDuration = null, string videoUrl = "",
            string certificateUrl = "", string storeUrl = "", string ecommerceUrl = "", bool createInEcommerce = false,
            List<Tag> tags = null, List<ObjectId> tutorsIds = null, List<ObjectId> extraInstructorIds = null, 
            ModuleGradeTypeEnum moduleGradeType = ModuleGradeTypeEnum.SubjectsLevel
        ) {
            var module = new ModuleDraft()
            {
                Id = ObjectId.GenerateNewId(),
                ModuleId = moduleId,
                Title = title,
                Excerpt = excerpt,
                InstructorId = instructorId.HasValue ? instructorId : ObjectId.Empty,
                Instructor = instructor,
                InstructorMiniBio = instructorMiniBio,
                ExtraInstructorIds = extraInstructorIds,
                ImageUrl = imageUrl,
                InstructorImageUrl = instructorImageUrl,
                Published = published,
                Duration = duration,
                Tags = tags,
                CertificateUrl = certificateUrl,
                TutorsIds = tutorsIds,
                StoreUrl = storeUrl,
                EcommerceUrl = ecommerceUrl,
                VideoDuration = videoDuration,
                VideoUrl = videoUrl,
                CreateInEcommerce = createInEcommerce,
                DraftPublished = false,
                ModuleGradeType = moduleGradeType
            };

            return Result.Ok(module);
        }

        public static Result<bool> IsInstructor(ModuleDraft module, ObjectId userId)
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

        public void PublishDraft()
        {
            DraftPublished = true;
            DraftPlubishedAt = DateTimeOffset.Now;
        }

        private ModuleDraft() : base()
        {
            SupportMaterials = new List<SupportMaterial>();
            Requirements = new List<Requirement>();
            Tags = new List<Tag>();
            Subjects = new List<Subject>();
        }
    }

    public class EcommerceModuleDraft
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

        public static Result<EcommerceModuleDraft> Create(
            long ecommerceId, int usersAmount, bool disableEcommerce, string price,
            bool disableFreeTrial, string linkEcommerce, string linkProduct,
            string subject, string hours
        )
        {
            return Result.Ok(
                new EcommerceModuleDraft
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

    public class ManageModuleWeights
    {
        public string Content { get; set; }
        public int Weight { get; set; }

        public static Result<ManageModuleWeights> Create(
            string content, int weight
        )
        {
            return Result.Ok(
                new ManageModuleWeights
                {
                    Content = content,
                    Weight = weight
                }
            );
        }
    }
}