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
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums.Queries
{
    public class GetEventForumPreviewByEventScheduleQuery
    {
        public class Contract : CommandContract<Result<List<EventForumQuestion>>>
        {
            public string EventScheduleId { get; set; }
            public int PageSize { get; set; }
        }

        public class ForumItem
        {
            public ObjectId EventScheduleId { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<EventForumQuestion>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<EventForumQuestion>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    List<EventForumQuestion> questions = new List<EventForumQuestion>();

                    ObjectId esId = ObjectId.Parse(request.EventScheduleId);
                    var query = await _db.Database
                        .GetCollection<ForumItem>("EventForums")
                        .FindAsync(x => x.EventScheduleId == esId);

                    var forum = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (forum == null)
                    {
                        return Result.Ok(questions);
                    }

                    var questionsList = await (await _db.Database
                    .GetCollection<EventForumQuestion>("EventForumQuestions")
                    .FindAsync(x => x.EventScheduleId == esId))
                    .ToListAsync();

                    if (questionsList.Count == 0)
                    {
                        return Result.Ok(questions);
                    }

                    foreach (ObjectId questionId in forum.Questions)
                    {
                        EventForumQuestion question = questionsList.FirstOrDefault(x => x.Id == questionId);
                        if (question != null)
                        {
                            question.Answers = await (await _db.Database
                            .GetCollection<EventForumAnswer>("EventForumAnswers")
                            .FindAsync(x => x.QuestionId == questionId))
                            .ToListAsync();
                            questions.Add(question);
                        }
                    }

                    var orderList = questions.OrderByDescending(x => x.LikedBy.Count).ThenByDescending(x => x.CreatedAt).ToList();

                    return Result.Ok(orderList.Take(request.PageSize).ToList());
                    
                }
                catch (Exception err)
                {
                    return Result.Fail<List<EventForumQuestion>>(err.Message);
                }
            }
        }
    }
}
