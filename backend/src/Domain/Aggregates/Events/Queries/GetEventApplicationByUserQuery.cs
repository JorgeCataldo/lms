using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Events.Queries
{
    public class GetEventApplicationByUserQuery
    {
        public class Contract : CommandContract<Result<EventApplicationItem>>
        {
            public string EventId { get; set; }
            public string ScheduleId { get; set; }
            public string UserId { get; set; }
        }

        public class EventApplicationItem
        {
            public ObjectId UserId { get; set; }
            public ObjectId? EventId { get; set; }
            public ObjectId? ScheduleId { get; set; }
            public PrepQuiz PrepQuiz { get; set; }
            public List<string> PrepQuizAnswers { get; set; }
            public ApplicationStatus ApplicationStatus { get; set; }
            public decimal? FinalGrade { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<EventApplicationItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<EventApplicationItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var evtId = ObjectId.Parse(request.EventId);
                var userId = ObjectId.Parse(request.UserId);
                var schId = ObjectId.Parse(request.ScheduleId);

                var dbApplication = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => x.UserId == userId && x.EventId == evtId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                var dbApplicationCount = await _db.EventApplicationCollection
                    .AsQueryable()
                    .Where(x => x.EventId == evtId && x.ScheduleId == schId)
                    .CountAsync(cancellationToken: cancellationToken);

                var eventSchedules = await _db.EventCollection
                    .AsQueryable()
                    .Where(x => x.Id == evtId)
                    .Select(x => x.Schedules)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                var limit = 0;

                if (eventSchedules != null)
                {
                    var eventSchedule = eventSchedules.FirstOrDefault(x => x.Id == schId);
                    if (eventSchedule != null)
                    {
                        limit = eventSchedule.ApplicationLimit ?? 0;
                    }
                }

                if (dbApplication == null)
                {
                    if (limit != 0 && dbApplicationCount > limit)
                    {
                        return Result.Ok(new EventApplicationItem
                        {
                            EventId = null,
                            ApplicationStatus = ApplicationStatus.Full
                        });
                    }
                    else
                    {
                        return Result.Ok(new EventApplicationItem
                        {
                            EventId = null
                        });
                    }
                }

                return Result.Ok(new EventApplicationItem()
                {
                    UserId = userId,
                    EventId = evtId,
                    ScheduleId = dbApplication.ScheduleId,
                    ApplicationStatus = dbApplication.ApplicationStatus,
                    PrepQuizAnswers = dbApplication.PrepQuizAnswers,
                    PrepQuiz = dbApplication.PrepQuiz,
                    FinalGrade = GetFinalGrade(dbApplication)
                });
            }

            private decimal? GetFinalGrade(EventApplication application)
            {
                if (application.InorganicGrade.HasValue && application.OrganicGrade.HasValue)
                {
                    var sum = application.InorganicGrade.Value + application.OrganicGrade.Value;
                    return Math.Round(sum / 2, 2);
                }
                return null;
            }
        }
    }
}
