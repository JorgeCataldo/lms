using System;
using System.Collections.Generic;
using Domain.Aggregates.Locations;
using Domain.Aggregates.Users;
using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events
{
    public class EventSchedule: Entity
    {
        public EventSchedule(
            DateTimeOffset eventDate,
            DateTimeOffset subscriptionStartDate, DateTimeOffset subscriptionEndDate,
            int duration, bool published, string webinarUrl, DateTimeOffset? forumStartDate = null, 
            DateTimeOffset? forumEndDate = null, int? applicationLimit = null, Location location = null
        ) {
            Id = ObjectId.GenerateNewId();
            EventDate = eventDate;
            SubscriptionStartDate = subscriptionStartDate;
            SubscriptionEndDate = subscriptionEndDate;
            Duration = duration;
            Published = published;
            WebinarUrl = webinarUrl;
            ForumStartDate = forumStartDate;
            ForumEndDate = forumEndDate;
            Location = location;
            ApplicationLimit = applicationLimit;
        }

        public HumanResources Instructor { get; set; }
        
        public List<ObjectId> Applicants { get; set; }
        public List<ObjectId> Registered { get; set; }
        public List<ObjectId> NotApproved { get; set; }
        
        public DateTimeOffset EventDate {get;set; }
        public string WebinarUrl { get; set; }

        public DateTimeOffset SubscriptionStartDate {get;set;}
        public DateTimeOffset SubscriptionEndDate {get;set; }
        public DateTimeOffset? ForumStartDate { get; set; }
        public DateTimeOffset? ForumEndDate { get; set; }
        public int Duration { get; set; }
        
        public bool Published { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public ObjectId? FinishedBy { get; set; }
        public bool SentReactionEvaluationEmails { get; set; }
        public Location Location { get; set; }
        public int? ApplicationLimit { get; set; }

        public static Result<EventSchedule> Create(
            DateTimeOffset eventDate,
            DateTimeOffset subscriptionStartDate, DateTimeOffset subscriptionEndDate,
            int duration, bool published, string webinarUrl,             
            DateTimeOffset? forumStartDate = null, DateTimeOffset? forumEndDate = null, int? applicationLimit = null, Location location = null
        ) {
            if (subscriptionStartDate >= subscriptionEndDate)
                return Result.Fail<EventSchedule>("Data de final de incrição deve ser maior que a inicial");

            if (subscriptionStartDate >= eventDate)
                return Result.Fail<EventSchedule>("Data de inicial de incrição deve ser anterior a data do evento");

            if (forumStartDate.HasValue && forumEndDate.HasValue && forumStartDate >= forumEndDate)
                return Result.Fail<EventSchedule>("Data de final de forum deve ser maior que a inicial");

            if (forumStartDate.HasValue && forumStartDate >= eventDate)
                return Result.Fail<EventSchedule>("Data de inicial de forum deve ser anterior a data do evento");

            var newObj = new EventSchedule(
                eventDate, subscriptionStartDate, subscriptionEndDate,
                duration, published, webinarUrl, forumStartDate, forumEndDate, 
                applicationLimit, location
            );

            return Result.Ok(newObj);
        }
    }
}