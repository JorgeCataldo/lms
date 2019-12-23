using System.Collections.Generic;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules
{
    public enum ContentType
    {
        Video = 0,
        Text = 1,
        Pdf = 2
    }
    public class Content : Entity
    {
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Value { get; set; }
        public long Duration { get; set; }
        public int? NumPages { get; set; }
        public ContentType Type { get; set; }
        public List<string> ReferenceUrls { get; set; }
        public List<ConceptPosition> Concepts { get; set; }
        
        public static Result<Content> Create(string title, string excerpt, List<ConceptPosition> concepts,
            IEnumerable<string> referenceUrls, string value, ContentType type, long duration, int? numPages = null)
        {
            if (title.Length > 200)
                return Result.Fail<Content>($"Tamanho máximo do título do conteúdo é de 200 caracteres. ({title})");

            var newContent = new Content(title, excerpt, value, type, referenceUrls, duration, numPages);
            foreach (var concept in concepts)
            {
                var res = ConceptPosition.Create(concept.Name, concept.Positions, concept.Anchors);
                if (res.IsFailure)
                    return Result.Fail<Content>(res.Error);
                newContent.Concepts.Add(res.Data);
            }

            return Result.Ok(newContent);
        }
        
        public Content(string title, string excerpt, string value, ContentType type,
            IEnumerable<string> referenceUrls, long duration, int? numPages = null) : base()
        {
            Id = ObjectId.GenerateNewId();
            this.Title = title;
            this.Excerpt = excerpt;
            this.Value = value;
            this.Type = type;
            Duration = duration;
            this.ReferenceUrls = new List<string>(referenceUrls);
            this.Concepts = new List<ConceptPosition>();
            this.NumPages = numPages;
        }
    }
}