using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Aggregates.Modules;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class EventDraft : Entity, IAggregateRoot
    {
        public ObjectId EventId { get; set; }
        public bool DraftPublished { get; set; }
        public DateTimeOffset DraftPlubishedAt { get; set; }

        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string ImageUrl { get; set; }
        public ObjectId? InstructorId { get; set; }
        public string Instructor { get; set; }
        public string InstructorMiniBio { get; set; }
        public string InstructorImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public Duration VideoDuration { get; set; }
        public Duration Duration { get; set; }
        public PrepQuiz PrepQuiz { get; set; }
        public List<PrepQuizQuestion> PrepQuizQuestionList { get; set; }
        public string CertificateUrl { get; set; }
        public string StoreUrl { get; set; }
        public bool CreateInEcommerce { get; set; }
        public long? EcommerceId { get; set; }
        public bool? ForceProblemStatement { get; set; }

        public List<Tag> Tags { get; set; }
        public List<EventSchedule> Schedules { get; set; }
        public List<SupportMaterial> SupportMaterials { get; set; }
        public List<Requirement> Requirements { get; set; }
        public List<ObjectId> TutorsIds { get; set; }

        public static Result<EventDraft> Create(
            ObjectId eventId, string title, string excerpt, string imageUrl,
            ObjectId? instructorId, string instructor, string instructorMiniBio, string instructorImageUrl,
            string[] tags, string videoUrl, Duration videoDuration, Duration duration,
            string certificateUrl, List<ObjectId> tutorsIds, string storeUrl = "", bool? forceProblemStatement = false
        ) {
            if (title.Length > 200)
                return Result.Fail<EventDraft>($"Tamanho máximo do título do evento é de 200 caracteres");

            var tagResults = tags.Select(Tag.Create).ToArray();
            var combinedResults = Result.Combine(tagResults);
            if (combinedResults.IsFailure)
                return Result.Fail<EventDraft>(combinedResults.Error);

            if (tags
                .GroupBy(x => x)
                .Select(x => new { x.Key, count = x.Count() })
                .Any(x => x.count > 1))
                return Result.Fail<EventDraft>("Não podem haver tags repetidos");

            var newObj = new EventDraft() {
                Id = ObjectId.GenerateNewId(),
                EventId = eventId,
                Title = title,
                Excerpt = excerpt,
                ImageUrl = imageUrl,
                InstructorId = instructorId,
                Instructor = instructor,
                InstructorMiniBio = instructorMiniBio,
                InstructorImageUrl = instructorImageUrl,
                Tags = tagResults.Select(x => x.Data).ToList(),
                VideoUrl = videoUrl,
                VideoDuration = videoDuration,
                Duration = duration,
                SupportMaterials = new List<SupportMaterial>(),
                Schedules = new List<EventSchedule>(),
                Requirements = new List<Requirement>(),
                CertificateUrl = certificateUrl,
                TutorsIds = tutorsIds,
                StoreUrl = storeUrl,
                DraftPublished = false,
                ForceProblemStatement = forceProblemStatement
            };

            return Result.Ok(newObj);
        }

        public void PublishDraft()
        {
            DraftPublished = true;
            DraftPlubishedAt = DateTimeOffset.Now;
        }
    }
}