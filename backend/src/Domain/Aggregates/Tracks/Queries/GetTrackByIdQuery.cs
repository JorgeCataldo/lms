using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Tracks.Queries
{
    public class GetTrackByIdQuery
    {
        public class Contract : CommandContract<Result<TrackItem>>
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public bool GetApplications { get; set; } = false;
        }

        public class TrackItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string[] Tags { get; set; }
            public bool Published { get; set; }
            public string CertificateUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public bool MandatoryCourseVideo { get; set; }
            public string CourseVideoUrl { get; set; }
            public int? CourseVideoDuration { get; set; }
            public string StoreUrl { get; set; }
            public string EcommerceUrl { get; set; }
            public bool RequireUserCareer { get; set; }
            public int? AllowedPercentageWithoutCareerInfo { get; set; }
            public ObjectId? ProfileTestId { get; set; }
            public string ProfileTestName { get; set; }
            public int? ValidFor { get; set; }

            public List<TrackEventItem> EventsConfiguration { get; set; }
            public List<TitleItem> Events { get; set; }
            public List<TrackModuleItem> ModulesConfiguration { get; set; }
            public List<ModuleItem> Modules { get; set; }
            public List<CalendarEventItem> CalendarEvents { get; set; }
            public List<EcommerceProduct> EcommerceProducts { get; set; }

            public TrackItem()
            {
                Events = new List<TitleItem>();
            }
        }
        
        public class TrackModuleItem
        {
            public string Title { get; set; }
            public ObjectId ModuleId { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public int Order { get; set; }
            public decimal Weight { get; set; }
            public DateTimeOffset? CutOffDate { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class TrackEventItem
        {
            public string Title { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public DateTimeOffset? EventDate { get; set; }
            public int Duration { get; set; }
            public int Order { get; set; }
            public string ImageUrl { get; set; }
            public bool? HasTakenPart { get; set; }
            public decimal Weight { get; set; }
            public bool AlwaysAvailable { get; set; }
            public DateTimeOffset? OpenDate { get; set; }
            public DateTimeOffset? ValuationDate { get; set; }
        }

        public class TitleItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public DateTimeOffset? Date { get; set; }
            public List<EventScheduleItem> Schedules { get; set; }
        }

        public class EventScheduleItem
        {
            public DateTimeOffset EventDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
            public int Duration { get; set; }
        }

        public class ModuleItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string Instructor { get; set; }
            public string ImageUrl { get; set; }
        }

        public class CalendarEventItem
        {
            public int? Duration { get; set; }
            public string Title { get; set; }
            public DateTimeOffset EventDate { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<TrackItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<TrackItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var trackId = ObjectId.Parse(request.Id);
                    var qry = await _db
                        .Database
                        .GetCollection<TrackItem>("Tracks")
                        .FindAsync(x => x.Id == trackId, cancellationToken: cancellationToken);
                    

                    var track =
                        await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    track.Modules = new List<ModuleItem>();

                    foreach (var req in track.ModulesConfiguration)
                    {
                        req.Title = (await (await _db
                                .Database
                                .GetCollection<TitleItem>("Modules")
                                .FindAsync(x => x.Id == req.ModuleId, cancellationToken: cancellationToken))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken)).Title;

                        track.Modules.Add(
                            (await (await _db
                                .Database
                                .GetCollection<ModuleItem>("Modules")
                                .FindAsync(x => x.Id == req.ModuleId, cancellationToken: cancellationToken))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken))
                        );
                    }
                    
                    foreach (var req in track.EventsConfiguration)
                    {
                        var query = await _db.Database
                            .GetCollection<TitleItem>("Events")
                            .FindAsync(x =>
                                x.Id == req.EventId,
                                cancellationToken: cancellationToken
                            );

                        var eventItem = await query.FirstOrDefaultAsync(
                            cancellationToken: cancellationToken
                        );

                        req.ImageUrl = eventItem.ImageUrl;

                        if (eventItem.Schedules != null && eventItem.Schedules.Count > 0)
                        {
                            eventItem.Date = eventItem.Schedules[0].EventDate;
                            req.EventDate = eventItem.Date;
                            req.Duration = eventItem.Schedules[0].Duration;
                        }
                        else
                            eventItem.Date = null;

                        if (eventItem != null) 
                            track.Events.Add(eventItem);

                        if (request.GetApplications)
                        {
                            req.HasTakenPart = await CheckApplication(
                                ObjectId.Parse(request.UserId),
                                req.EventScheduleId,
                                cancellationToken
                            );
                        }
                    }

                    return Result.Ok(track);
                }
                catch (Exception err)
                {
                    return Result.Fail<TrackItem>($"Ocorreu um erro ao buscar as questões: {err.Message}");
                }
            }

            private async Task<bool> CheckApplication(
                ObjectId studentId, ObjectId scheduleId, CancellationToken token
            ) {
                var query = await _db.Database
                        .GetCollection<EventApplication>("EventApplications")
                        .FindAsync(x =>
                            x.UserId == studentId &&
                            x.ScheduleId == scheduleId &&
                            x.UserPresence.HasValue &&
                            x.UserPresence == true,
                            cancellationToken: token
                        );
                return await query.AnyAsync(cancellationToken: token);
            }
        }
    }
}
