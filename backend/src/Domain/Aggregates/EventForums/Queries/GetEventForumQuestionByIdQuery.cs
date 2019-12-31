using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.EventForums.Queries
{
    public class GetEventForumQuestionByIdQuery
    {
        public class Contract : CommandContract<Result<PagedQuestionItem>>
        {
            public string QuestionId { get; set; }
            public string EventScheduleId { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public string UserId { get; set; }
        }

        public class PagedQuestionItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public long ItemsCount { get; set; }
            public string Position { get; set; }
            public bool IsInstructor { get; set; }
            public List<ForumAnswerItem> Answers { get; set; }
        }

        public class ForumAnswerItem
        {
            public ObjectId Id { get; set; }
            public ObjectId QuestionId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string Text { get; set; }
            public bool Liked { get; set; }
            public List<string> LikedBy { get; set; }
            public string UserName { get; set; }
            public string UserImgUrl { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<PagedQuestionItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<PagedQuestionItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.QuestionId))
                        return Result.Fail<PagedQuestionItem>("Id da Pergunta não informado");

                    var question = await GetForumQuestion(request.QuestionId, cancellationToken);

                    if (question == null)
                        return Result.Fail<PagedQuestionItem>("Pergunta não encontrada");

                    var options = new FindOptions<ForumAnswerItem>() {
                        Limit = request.PageSize,
                        Skip = (request.Page - 1) * request.PageSize
                    };

                    var builder = Builders<ForumAnswerItem>.Filter;
                    var filters = builder.Eq(
                        "questionId", ObjectId.Parse(request.QuestionId)
                    );

                    var collection = _db.Database.GetCollection<ForumAnswerItem>("EventForumAnswers");

                    var query = await collection.FindAsync(
                        filters, options: options,
                        cancellationToken: cancellationToken
                    );

                    var answers = await query.ToListAsync(cancellationToken);

                    foreach (var answer in answers)
                        answer.Liked = answer.LikedBy.Any(uId => uId == request.UserId);

                    return Result.Ok(
                        new PagedQuestionItem()
                        {
                            Id = request.QuestionId,
                            Title = question.Title,
                            Description = question.Description,
                            Answers = answers,
                            IsInstructor = await CheckEventInstructor(
                                ObjectId.Parse(request.EventScheduleId),
                                ObjectId.Parse(request.UserId),
                                cancellationToken
                            ),
                            ItemsCount = await collection.CountDocumentsAsync(filters),
                            Position = question.Position
                        }
                    );
                }
                catch (Exception err)
                {
                    return Result.Fail<PagedQuestionItem>(err.Message);
                }
            }

            private async Task<EventForumQuestion> GetForumQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);
                var query = await _db.Database
                    .GetCollection<EventForumQuestion>("EventForumQuestions")
                    .FindAsync(x => x.Id == questionId);

                return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            }

            private async Task<bool> CheckEventInstructor(
                ObjectId eventScheduleId, ObjectId userId, CancellationToken cancellationToken
            )
            {
                var query = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x =>
                        x.Schedules.Any(y => y.Id == eventScheduleId) &&
                        x.InstructorId.HasValue &&
                        x.InstructorId.Value == userId,
                        cancellationToken: cancellationToken
                    );
                return await query.AnyAsync();
            }
        }
    }
}
