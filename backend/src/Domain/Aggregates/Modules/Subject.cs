using System.Collections.Generic;
using Domain.Extensions;
using Domain.SeedWork;
using Domain.ValueObjects;
using MailChimp.Net.Core;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tg4.Infrastructure.Functional;
using Result = Tg4.Infrastructure.Functional.Result;

namespace Domain.Aggregates.Modules
{
    public class Subject: Entity
    {
        public string Title { get; set; }
        public string Excerpt { get; set; }

        public List<Concept> Concepts { get; set; }
        public List<Content> Contents { get; set; }

        [JsonConverter(typeof(ObjectIdConverter))]
        public List<ObjectId> Questions { get; set; }
        public List<ValueObjects.UserProgress> UserProgresses { get; set; }

        public static Result<Subject> Create(string title, string excerpt, string[] concepts)
        {
            if (title.Length > 200)
                return Result.Fail<Subject>($"Tamanho máximo do título do assunto é de 200 caracteres. ({title})");

            var newSubject = new Subject(title, excerpt);
            var clist = Concept.GetConcepts(concepts);

            if (clist.IsFailure) 
                return Result.Fail<Subject>(clist.Error);

            newSubject.Concepts = clist.Data;

            return Result.Ok(newSubject);
        }

        public Subject(string title, string excerpt) : base()
        {
            Id = ObjectId.GenerateNewId();
            this.Title = title;
            this.Excerpt = excerpt;
            this.Concepts = new List<Concept>();
            this.Contents = new List<Content>();
            this.Questions = new List<ObjectId>();
            this.UserProgresses = new List<ValueObjects.UserProgress>();
        }
    }
}