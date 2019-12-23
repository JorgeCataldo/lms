using System;
using System.Collections.Generic;
using System.Linq;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks
{
    public class Track : Entity
    {
        private Track(string title, string description, string imageUrl,
            List<TrackEvent> events, List<TrackModule> trackModules,
            int studentsCount = 0, decimal duration = 0, List<Tag> tags = null,
            bool published = false, string certificateUrl = "", string storeUrl = "", string ecommerceUrl = "",
            ObjectId? profileTestId = null, string profileTestName = null, int? validFor = null
        ) {
            Id = ObjectId.GenerateNewId();
            Description = description;
            ImageUrl = imageUrl;
            Title = title;
            EventsConfiguration = events ?? new List<TrackEvent>();
            ModulesConfiguration = trackModules ?? new List<TrackModule>();
            StudentsCount = studentsCount;
            Duration = duration;
            Tags = tags;
            Published = published;
            CertificateUrl = certificateUrl;
            StoreUrl = storeUrl;
            EcommerceUrl = EcommerceUrl;
            ProfileTestId = profileTestId;
            ProfileTestName = profileTestName;
            ValidFor = validFor;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<Tag> Tags { get; set; }
        public bool Published { get; set; }
        public bool RequireUserCareer { get; set; }
        public int? AllowedPercentageWithoutCareerInfo { get; set; }

        public List<TrackEvent> EventsConfiguration { get; set; }
        public List<TrackModule> ModulesConfiguration { get; set; }
        public decimal Duration { get; set; }
        public int StudentsCount { get; set; }
        public string CertificateUrl { get; set; }
        public string VideoUrl { get; set; }
        public int? VideoDuration { get; set; }
        public bool MandatoryCourseVideo { get; set; }
        public string CourseVideoUrl { get; set; }
        public int? CourseVideoDuration { get; set; }
        public List<CalendarEvent> CalendarEvents { get; set; }
        public string StoreUrl { get; set; }
        public string EcommerceUrl { get; set; }
        public ObjectId? ProfileTestId { get; set; }
        public string ProfileTestName { get; set; }
        public long? EcommerceId { get; set; } // Deprecated
        public List<EcommerceProduct> EcommerceProducts { get; set; }
        public int? ValidFor { get; set; }

        public static Result<Track> Create(
            string title, string description, string imageUrl,
            List<TrackEvent> events, List<TrackModule> modulesConfiguration,
            int studentsCount = 0, decimal duration = 0, List<Tag> tags = null,
            bool published = false, string certificateUrl = "", string storeUrl = "", string ecommerceUrl = "",
            ObjectId? profileTestId = null, string profileTestName = null, int? validFor = null
        ) {
            if (title.Length > 200)
                return Result.Fail<Track>($"Tamanho máximo do título da trilha é de 200 caracteres. ({title})");

            if (events
                .GroupBy(x => x.EventId)
                .Select(x => new { x.Key, count = x.Count() })
                .Any(x => x.count > 1))
                return Result.Fail<Track>("Não podem haver eventos repetidos");

            if (modulesConfiguration
                .GroupBy(x => x.ModuleId)
                .Select(x => new { x.Key, count = x.Count() })
                .Any(x => x.count > 1))
                return Result.Fail<Track>("Não podem haver módulos repetidos");

            var newObject = new Track(
                title, description, imageUrl,
                events, modulesConfiguration,
                studentsCount, duration, tags,
                published, certificateUrl, storeUrl, ecommerceUrl,
                profileTestId, profileTestName, validFor
            );

            return Result.Ok(newObject);
        }
    }

    public class TrackModule
    {
        private TrackModule(
            ObjectId moduleId, string title, int level, decimal percentage,
            int order, decimal weight, DateTimeOffset? cutOffDate, decimal bdqWeight, decimal evaluationWeight,
            bool alwaysAvailable, DateTimeOffset? openDate, DateTimeOffset? valuationDate
        )
        {
            ModuleId = moduleId;
            Title = title;
            Level = level;
            Percentage = percentage;
            Order = order;
            Weight = weight;
            CutOffDate = cutOffDate;
            BDQWeight = bdqWeight;
            EvaluationWeight = evaluationWeight;
            AlwaysAvailable = alwaysAvailable;
            OpenDate = openDate;
            ValuationDate = valuationDate;
        }

        public ObjectId ModuleId { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public decimal Percentage { get; set; }
        public int Order { get; set; }
        public decimal Weight { get; set; }
        public DateTimeOffset? CutOffDate { get; set; }
        public decimal BDQWeight { get; set; }
        public decimal EvaluationWeight { get; set; }
        public bool? AlwaysAvailable { get; set; }
        public DateTimeOffset? OpenDate { get; set; }
        public DateTimeOffset? ValuationDate { get; set; }

        public static Result<TrackModule> Create(
            ObjectId moduleId, string title, int level, decimal percentage,
            int order, decimal weight, DateTimeOffset? cutOffDate, decimal bdqWeight, decimal evaluationWeight,
            bool alwaysAvailable, DateTimeOffset? openDate, DateTimeOffset? valuationDate
        )
        {
            if (order < 0)
                return Result.Fail<TrackModule>("Ordem não pode ser menor que zero");

            if (percentage < 0 || percentage > 1)
                return Result.Fail<TrackModule>("Percentual deve ser maior que zero e menor ou igual a 1");

            var levels = Levels.Level.GetAllLevels().Data;
            if (!levels.Any(x => x.Id == level))
                return Result.Fail<TrackModule>("Nivel de dificuldade não existe");

            var newObject = new TrackModule(moduleId, title, level, percentage, order, weight, cutOffDate, bdqWeight, evaluationWeight,
            alwaysAvailable, openDate, valuationDate);
            return Result.Ok(newObject);
        }
    }

    public class TrackEvent
    {
        private TrackEvent(ObjectId eventId, ObjectId scheduleId, int order,
            string title, decimal weight, DateTimeOffset? cutOffDate, bool alwaysAvailable, 
            DateTimeOffset? openDate, DateTimeOffset? valuationDate)
        {
            EventId = eventId;
            EventScheduleId = scheduleId;
            Order = order;
            Title = title;
            Weight = weight;
            CutOffDate = cutOffDate;
            AlwaysAvailable = alwaysAvailable;
            OpenDate = openDate;
            ValuationDate = valuationDate;
        }

        public ObjectId EventId { get; set; }
        public ObjectId EventScheduleId { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public decimal Weight { get; set; }
        public DateTimeOffset? CutOffDate { get; set; }
        public bool AlwaysAvailable { get; set; }
        public DateTimeOffset? OpenDate { get; set; }
        public DateTimeOffset? ValuationDate { get; set; }

        public static Result<TrackEvent> Create(
            ObjectId eventId, ObjectId scheduleId, int order,
            string title, decimal weight, DateTimeOffset? cutOffDate,
            bool alwaysAvailable, DateTimeOffset? openDate, DateTimeOffset? valuationDate
        )
        {
            if (order < 0)
                return Result.Fail<TrackEvent>("Ordem não pode ser menor que zero");

            var newObject = new TrackEvent(eventId, scheduleId, order, title, weight, cutOffDate,
            alwaysAvailable, openDate, valuationDate);
            return Result.Ok(newObject);
        }
    }

    public class CalendarEvent
    {
        public int? Duration { get; set; }
        public string Title { get; set; }
        public DateTimeOffset EventDate { get; set; }

        public static Result<CalendarEvent> Create(
            string title, DateTimeOffset eventDate, int? Duration = null
        )
        {
            return Result.Ok(new CalendarEvent
            {
                Title = title,
                EventDate = eventDate,
                Duration = Duration
            });
        }
    }

    public class EcommerceProduct
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

        public static Result<EcommerceProduct> Create(
            long ecommerceId, int usersAmount, bool disableEcommerce, string price,
            bool disableFreeTrial, string linkEcommerce, string linkProduct,
            string subject, string hours
        ) {
            return Result.Ok(
                new EcommerceProduct {
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