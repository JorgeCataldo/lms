using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Locations;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Modules.SupportMaterial;

namespace Domain.Aggregates.Events.Queries
{
    public class GetEventByIdQuery
    {
        public class Contract : CommandContract<Result<EventItem>>
        {
            public string Id { get; set; }
            public string SimpleQuery { get; set; }
        }

        public class EventItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Excerpt {get;set;}
            public string ImageUrl { get; set; }
            public ObjectId? InstructorId { get; set; }
            public string Instructor { get; set; }
            public string InstructorMiniBio { get; set; }
            public string InstructorImageUrl { get; set; }
            public string VideoUrl { get; set; }
            public int? VideoDuration { get; set; }
            public int? Duration { get; set; }
            public string[] Tags { get; set; }
            public List<EventScheduleItem> Schedules { get; set; }
            public List<SupportMaterialItem> SupportMaterials { get; set; }
            public List<RequirementItem> Requirements { get; set; }
            public PrepQuizItem PrepQuiz { get; set; }
            public List<PrepQuizQuestion> PrepQuizQuestionList { get; set; }
            public string[] PrepQuizQuestions { get; set; }
            public string CertificateUrl { get; set; }
            public List<ObjectId> TutorsIds { get; set; }
            public List<TutorInfo> Tutors { get; set; }
            public string StoreUrl { get; set; }
            public long? EcommerceId { get; set; }
            public bool? ForceProblemStatement { get; set; }

            public EventItem()
            {
                Schedules = new List<EventScheduleItem>();
            }
        }

        public class EventScheduleItem
        {
            public ObjectId Id { get; set; }
            public DateTimeOffset EventDate { get; set; }
            public DateTimeOffset SubscriptionStartDate { get; set; }
            public DateTimeOffset SubscriptionEndDate { get; set; }
            public DateTimeOffset? ForumStartDate { get; set; }
            public DateTimeOffset? ForumEndDate { get; set; }
            public int? Duration { get; set; }
            public bool Published { get; set; }
            public int? UsersTotal { get; set; }
            public int? ApprovedUsersTotal { get; set; }
            public int? RejectedUsersTotal { get; set; }
            public DateTimeOffset? FinishedAt { get; set; }
            public ObjectId? FinishedBy { get; set; }
            public string WebinarUrl { get; set; }
            public Location Location { get; set; }
            public int? ApplicationLimit { get; set; }
        }

        public class SupportMaterialItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public string ImageUrl { get; set; }
            public SupportMaterialTypeEnum? Type { get; set; }
        }

        public class RequirementItem
        {
            public ObjectId ModuleId { get; set; }
            public string Title { get; set; }
            public bool? Optional { get; set; }
            public int? Level { get; set; }
            public decimal? Percentage { get; set; }
            public ContractUserProgress RequirementValue { get; set; }
        }

        public class ContractUserProgress
        {
            public int Level { get; set; }
            public decimal Percentage { get; set; }
        }

        public class ModuleNameItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
        }

        public class PrepQuizItem
        {
            public ObjectId Id { get; set; }
            public List<string> Questions { get; set; }
        }
        public class TutorInfo
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<EventItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<EventItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var evtId = ObjectId.Parse(request.Id);
                var qry = await _db.Database
                    .GetCollection<EventItem>("Events")
                    .FindAsync(x => x.Id == evtId, cancellationToken: cancellationToken);
                    
                var evt = await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (evt == null)
                    return Result.Fail<EventItem>("Evento não existe");

                if (request.SimpleQuery != null && request.SimpleQuery == "true")
                    return Result.Ok(evt);

                evt.Requirements = evt.Requirements ?? new List<RequirementItem>();
                foreach (var req in evt.Requirements.ToList())
                {
                    var mod = (await (await _db
                            .Database
                            .GetCollection<ModuleNameItem>("Modules")
                            .FindAsync(x => x.Id == req.ModuleId, cancellationToken: cancellationToken))
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken));
                    if (mod == null)
                    {
                        evt.Requirements.Remove(req);
                        continue;
                    }

                    req.Title = mod.Title;
                    req.Level = req.RequirementValue.Level;
                    req.Percentage = req.RequirementValue.Percentage;
                    req.RequirementValue = null;
                }

                evt.Schedules = evt.Schedules ?? new List<EventScheduleItem>();
                foreach (var sch in evt.Schedules.ToList())
                {
                    var eveapp = _db
                            .Database
                            .GetCollection<EventApplication>("EventApplications")
                            .AsQueryable()
                            .Where(x => x.EventId == evtId && x.ScheduleId == sch.Id)
                            .ToList();

                    if (eveapp != null && eveapp.Count > 0)
                    {
                        sch.UsersTotal = eveapp.Count;
                        sch.ApprovedUsersTotal = eveapp.Where(x => x.ApplicationStatus == ApplicationStatus.Approved).ToList().Count;
                        sch.RejectedUsersTotal = eveapp.Where(x => x.ApplicationStatus == ApplicationStatus.Rejected).ToList().Count;
                    }
                    else
                    {
                        sch.UsersTotal = 0;
                        sch.ApprovedUsersTotal = 0;
                        sch.RejectedUsersTotal = 0;
                    }
                }

                //if (evt.PrepQuiz != null)
                //{
                //    evt.PrepQuizQuestions = evt.PrepQuiz.Questions.ToArray();
                //    evt.PrepQuiz = null;
                //}

                if (evt.PrepQuiz != null && (evt.PrepQuizQuestionList == null || evt.PrepQuizQuestionList.Count() == 0))
                {
                    evt.PrepQuizQuestionList = new List<PrepQuizQuestion>();
                    evt.PrepQuizQuestions = evt.PrepQuiz.Questions.ToArray();

                    for (int i = 0; i < evt.PrepQuizQuestions.Length; i++)
                    {
                        var prepQuizResult = PrepQuizQuestion.Create(evt.PrepQuizQuestions[i], false, evt.PrepQuizQuestions);
                        if (prepQuizResult.IsFailure)
                            return Result.Fail<EventItem>("Ocorreu um erro ao buscar as perguntas do evento.");

                        evt.PrepQuizQuestionList.Add(prepQuizResult.Data);
                    }

                    evt.PrepQuiz = null;
                }

                evt = await CheckTutors(evt, cancellationToken);


                return Result.Ok(evt);
            }

            private async Task<EventItem> CheckTutors(EventItem dbEvent, CancellationToken token)
            {
                dbEvent.TutorsIds = dbEvent.TutorsIds ?? new List<ObjectId>();
                dbEvent.Tutors = await _db.Database
                    .GetCollection<TutorInfo>("Users")
                    .AsQueryable()
                    .Where(x => dbEvent.TutorsIds.Contains(x.Id))
                    .ToListAsync();

                return dbEvent;
            }
        }        
    }
}
