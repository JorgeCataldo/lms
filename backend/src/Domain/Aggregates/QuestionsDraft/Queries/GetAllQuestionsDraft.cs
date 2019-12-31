using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.QuestionsDraft.Queries
{
    public class GetAllQuestionsDraft
    {
        public class Contract : CommandContract<Result<List<QuestionItem>>>
        {
            public string ModuleId { get; set; }
        }

        public class QuestionItem
        {
            public ObjectId DraftId { get; set; }
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId Id { get; set; }
            public string Text { get; set; }
            public List<AnswerItem> Answers { get; set; }
            public string[] Concepts { get; set; }
            public int Level { get; set; }
            public int Duration { get; set; }
            public DateTimeOffset DeletedAt { get; set; }
            public bool DraftPublished { get; set; }
        }

        public class AnswerItem
        {
            public string Description { get; set; }
            public int Points { get; set; }
            public List<AnswerConceptItem> Concepts { get; set; }
        }

        public class AnswerConceptItem
        {
            public string Concept { get; set; }
            public bool IsRight { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<QuestionItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<QuestionItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<List<QuestionItem>>("Id do Módulo não Informado");

                var modId = ObjectId.Parse(request.ModuleId);

                var questions = await _db.Database.GetCollection<QuestionItem>("QuestionsDrafts")
                    .AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.DraftId == modId || x.ModuleId == modId
                        )
                    )
                    .ToListAsync(cancellationToken: cancellationToken);

                return Result.Ok(questions);
            }
        }
    }
}
