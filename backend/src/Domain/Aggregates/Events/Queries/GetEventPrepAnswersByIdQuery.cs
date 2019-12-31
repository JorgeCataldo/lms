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
    public class GetEventPrepAnswersByIdQuery
    {
        public class Contract : CommandContract<Result<List<AnswerItem>>>
        {
            public string Id { get; set; }
            public string ScheduleId { get; set; }
        }

        public class AnswerItem
        {
            public string UserName { get; set; }
            public string EventName { get; set; }
            public string Question { get; set; }
            public string Answer {get;set;}
            public string ApplicationStatus { get; internal set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<List<AnswerItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<AnswerItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var evtId = ObjectId.Parse(request.Id);
                    var scheduleId = ObjectId.Parse(request.ScheduleId);
                    var evt = await _db
                        .EventCollection
                        .AsQueryable()
                        .Where(x => x.Id == evtId)
                        .Select(x=>x.Title)
                        .FirstOrDefaultAsync(cancellationToken);

                    var answers = await _db.EventApplicationCollection
                        .AsQueryable()
                        .Where(x => x.EventId == evtId && x.ScheduleId == scheduleId)
                        .Select(x=>new { x.UserId, x.PrepQuizAnswers, x.PrepQuiz, x.ApplicationStatus})
                        .ToListAsync(cancellationToken);

                    var userIds = answers.Select(x => x.UserId).ToArray();
                    var users = await _db.UserCollection
                        .AsQueryable()
                        .Where(x => userIds.Contains(x.Id))
                        .Select(x=>new { x.Id, x.Name })
                        .ToListAsync(cancellationToken);

                    var report =
                        from a in answers
                        join u in users on a.UserId equals u.Id
                        select new { u.Name, a.PrepQuiz, a.PrepQuizAnswers, a.ApplicationStatus };

                    var export = new List<AnswerItem>();
                    foreach(var x in report)
                    {
                        for(var i = 0; i<x.PrepQuizAnswers.Count; i++)
                        {
                            export.Add(new AnswerItem()
                            {
                                UserName = x.Name,
                                //Question = x.PrepQuiz.Questions[i],
                                Answer = x.PrepQuizAnswers[i],
                                EventName = evt,
                                ApplicationStatus = x.ApplicationStatus.ToString()
                            });
                        }
                    }

                    return Result.Ok(export);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<AnswerItem>>($"Ocorreu um erro ao buscar o evento: {err.Message}");
                }
            }
        }
    }
}
