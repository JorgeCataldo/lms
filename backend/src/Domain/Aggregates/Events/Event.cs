using System.Collections.Generic;
using System.Linq;
using Domain.Aggregates.Modules;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class Event: Entity, IAggregateRoot
    {
        public Event(
            string title, string excerpt, string imageUrl,
            ObjectId? instructorId, string instructor, string instructorMiniBio, string instructorImageUrl,
            List<Tag> tags, string videoUrl, Duration videoDuration, Duration duration,
            string certificateUrl, List<ObjectId> tutorsIds, string storeUrl = "", bool? forceProblemStatement = false
        ) {
            Id = ObjectId.GenerateNewId();
            Title = title;
            Excerpt = excerpt;
            ImageUrl = imageUrl;
            InstructorId = instructorId;
            Instructor = instructor;
            InstructorMiniBio = instructorMiniBio;
            InstructorImageUrl = instructorImageUrl;
            Tags = tags;
            VideoUrl = videoUrl;
            VideoDuration = videoDuration;
            Duration = duration;
            SupportMaterials = new List<SupportMaterial>();
            Schedules = new List<EventSchedule>();
            Requirements = new List<Requirement>();
            CertificateUrl = certificateUrl;
            TutorsIds = tutorsIds;
            StoreUrl = storeUrl;
            ForceProblemStatement = forceProblemStatement;
        }

        public string Title { get; set; }
        public string Excerpt {get;set;}
        public string ImageUrl { get; set; }
        public ObjectId? InstructorId { get; set; }
        public string Instructor { get; set; }
        public string InstructorMiniBio { get; set; }
        public string InstructorImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public Duration VideoDuration { get; set; }
        public Duration Duration { get; set; }
        public List<Tag> Tags { get; set; }
        public string StoreUrl { get; set; }
        public long? EcommerceId { get; set; }
        public bool? ForceProblemStatement { get; set; }

        public List<EventSchedule> Schedules { get; set; }
        public List<SupportMaterial> SupportMaterials { get; set; }
        public List<Requirement> Requirements { get; set; }

        /// <summary>
        /// Alteração feita por: Marcelo Gobira
        /// PrepQuiz:               Propriedade que possui uma lista de string contendo as perguntas para a aplicação em um evento.
        /// PrepQuizQuestionList:   Nova propriedade criada contendo a lista de perguntas para aplicação em um evento podendo 
        ///                         incluir um arquivo como resposta. Propriedade criada para evitar a perda de dados de "PrepQuiz"
        /// </summary>

        public PrepQuiz PrepQuiz { get; set; }
        public List<PrepQuizQuestion> PrepQuizQuestionList { get; set; }
        public string CertificateUrl { get; set; }
        public List<ObjectId> TutorsIds { get; set; }

        public static Result<Event> Create(
            string title, string excerpt, string imageUrl,
            ObjectId instructorId, string instructor, string instructorMiniBio, string instructorImageUrl,
            string[] tags, string videoUrl, int? videoDuration, int? duration,
            string certificateUrl, List<ObjectId> tutorsIds, string storeUrl = "", bool? forceProblemStatement = false
        ) {
            if (title.Length > 200)
                return Result.Fail<Event>($"Tamanho máximo do título do evento é de 200 caracteres. ({title})");

            Duration videoDurationObj = null;
            if (videoDuration.HasValue)
            {
                var result = Duration.Create(videoDuration.Value);
                if (result.IsFailure)
                    return Result.Fail<Event>(result.Error);
                videoDurationObj = result.Data;
            }
            
            Duration durationObj = null;
            if (duration.HasValue)
            {
                var result = Duration.Create(duration.Value);
                if (result.IsFailure)
                    return Result.Fail<Event>(result.Error);
                durationObj = result.Data;
            }

            var tagResults = tags.Select(Tag.Create).ToArray();
            var combinedResults = Result.Combine(tagResults);
            if (combinedResults.IsFailure)
                return Result.Fail<Event>(combinedResults.Error);
            
            if(tags
                .GroupBy(x => x)
                .Select(x => new { x.Key, count= x.Count() })
                .Any(x => x.count > 1))
                return Result.Fail<Event>("Não podem haver tags repetidos");

            var newObj = new Event(
                title, excerpt, imageUrl,
                instructorId, instructor, instructorMiniBio, instructorImageUrl,
                tagResults.Select(x=>x.Data).ToList(),
                videoUrl, videoDurationObj, durationObj,
                certificateUrl, tutorsIds, storeUrl, forceProblemStatement
            );

            return Result.Ok(newObj);
        }
    }
}