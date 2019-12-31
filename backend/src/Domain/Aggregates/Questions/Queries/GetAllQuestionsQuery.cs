using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules.Queries;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Questions.Queries
{
    public class GetAllQuestionsQuery
    {
        public class Contract : CommandContract<Result<List<QuestionItem>>>
        {
            public string ModuleId { get; set; }
        }

        public class QuestionItem
        {
            public ObjectId ModuleId { get; set; }
            public ObjectId SubjectId { get; set; }
            public ObjectId Id { get; set; }
            public string Text { get; set; }
            public List<AnswerItem> Answers { get; set; }
            public string[] Concepts { get; set; }
            public int Level { get; set; }
            public int Duration { get; set; }
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
                try
                {
                    var options = new FindOptions<QuestionItem>();

                    FilterDefinition<QuestionItem> filters = SetFilters(request);
                    var modulesCollection = _db.Database.GetCollection<QuestionItem>("Questions");

                    var qry = await modulesCollection.FindAsync(filters,
                        options: options,
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok(await qry.ToListAsync(cancellationToken));
                }
                catch (Exception err)
                {
                    return Result.Fail<List<QuestionItem>>($"Ocorreu um erro ao buscar as questões: {err.Message}");
                }
            }

            private FilterDefinition<QuestionItem> SetFilters(Contract request)
            {
                var filters = FilterDefinition<QuestionItem>.Empty;
                var builder = Builders<QuestionItem>.Filter;
                
                filters = filters & builder.Eq("moduleId", ObjectId.Parse(request.ModuleId));

                return filters;
            }
        }
    }
}

