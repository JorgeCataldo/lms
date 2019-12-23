using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Queries
{
    public class GetForumPreviewByModuleQuery
    {
        public class Contract : CommandContract<Result<List<ForumQuestion>>>
        {
            public string ModuleId { get; set; }
            public int PageSize { get; set; }
        }

        public class ForumItem
        {
            public ObjectId ModuleId { get; set; }
            public List<ObjectId> Questions { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ForumQuestion>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ForumQuestion>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    List<ForumQuestion> questions = new List<ForumQuestion>();

                    ObjectId mId = ObjectId.Parse(request.ModuleId);
                    var query = await _db.Database
                        .GetCollection<ForumItem>("Forums")
                        .FindAsync(x => x.ModuleId == mId);

                    var forum = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (forum == null)
                    {
                        return Result.Ok(questions);
                    }

                    var questionsList = await (await _db.Database
                    .GetCollection<ForumQuestion>("ForumQuestions")
                    .FindAsync(x => x.ModuleId == mId))
                    .ToListAsync();

                    if (questionsList.Count == 0)
                    {
                        return Result.Ok(questions);
                    }

                    foreach (ObjectId questionId in forum.Questions)
                    {
                        ForumQuestion question = questionsList.FirstOrDefault(x => x.Id == questionId);
                        if (question != null)
                        {
                            question.Answers = await (await _db.Database
                            .GetCollection<ForumAnswer>("ForumAnswers")
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
                    return Result.Fail<List<ForumQuestion>>(err.Message);
                }
            }
        }
    }
}
