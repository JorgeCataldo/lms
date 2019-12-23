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
    public class GetUserEventForumPreviewByEventScheduleQuery
    {
        public class Contract : CommandContract<Result<List<EventForumQuestionItem>>>
        {
            public string EventScheduleId { get; set; }
            public string UserId { get; set; }
        }

        public class ForumItem
        {
            public ObjectId EventScheduleId { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class EventForumQuestionItem
        {
            public ObjectId Id { get; set; }
            public ObjectId EventId { get; set; }
            public ObjectId EventScheduleId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public ObjectId CreatedBy { get; set; }
            public List<EventForumAnswer> Answers { get; set; }
            public string UserName { get; set; } = "";
            public string UserImgUrl { get; set; } = "";
        }

        public class Handler : IRequestHandler<Contract, Result<List<EventForumQuestionItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<EventForumQuestionItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    List<EventForumQuestionItem> questions = new List<EventForumQuestionItem>();

                    ObjectId esId = ObjectId.Parse(request.EventScheduleId);
                    ObjectId userId = ObjectId.Parse(request.UserId);
                    var query = await _db.Database
                        .GetCollection<ForumItem>("EventForums")
                        .FindAsync(x => x.EventScheduleId == esId);

                    var forum = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (forum == null)
                    {
                        return Result.Ok(questions);
                    }

                    var questionsList = await (await _db.Database
                    .GetCollection<EventForumQuestionItem>("EventForumQuestions")
                    .FindAsync(x => x.EventScheduleId == esId))
                    .ToListAsync();

                    if (questionsList.Count == 0)
                    {
                        return Result.Ok(questions);
                    }

                    foreach (ObjectId questionId in forum.Questions)
                    {
                        EventForumQuestionItem question = questionsList.FirstOrDefault(x => x.Id == questionId);
                        if (question != null)
                        {
                            question.Answers = await (await _db.Database
                            .GetCollection<EventForumAnswer>("EventForumAnswers")
                            .FindAsync(x => x.QuestionId == questionId && x.CreatedBy == userId))
                            .ToListAsync(cancellationToken);
                            questions.Add(question);
                        }
                    }

                    var forumQuestionUsers = new List<ObjectId>();
                    foreach (EventForumQuestionItem question in questions.Reverse<EventForumQuestionItem>())
                    {
                        if (question.Answers.Count == 0  && question.CreatedBy != userId)
                        {
                            questions.Remove(question);
                        }
                        else
                        {
                            forumQuestionUsers.Add(question.CreatedBy);
                        }
                    }
                    
                    if (forumQuestionUsers.Count > 0)
                    {
                        var users = await (await _db.Database
                            .GetCollection<UserItem>("Users")
                            .FindAsync(x => forumQuestionUsers.Contains(x.Id)))
                            .ToListAsync(cancellationToken);

                        questions = (from question in questions
                            join user in users on question.CreatedBy equals user.Id
                            select new EventForumQuestionItem()
                            {
                                 Id = question.Id,
                                 EventId = question.EventId,
                                 EventScheduleId = question.EventScheduleId,
                                 Title = question.Title,
                                 Description = question.Description,
                                 CreatedAt = question.CreatedAt,
                                 CreatedBy = question.CreatedBy,
                                 Answers = question.Answers,
                                 UserName = user.Name,
                                 UserImgUrl = user.ImageUrl
                            }).ToList();
                    }

                    var orderList = questions.OrderByDescending(x => x.CreatedAt).ToList();

                    return Result.Ok(orderList);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<EventForumQuestionItem>>(err.Message);
                }
            }
        }
    }
}
