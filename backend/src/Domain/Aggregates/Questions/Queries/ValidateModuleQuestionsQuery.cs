using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Levels;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Questions.Queries
{
    public class ValidateModuleQuestionsQuery
    {
        public class Contract : CommandContract<Result<List<InvalidSubjectItem>>>
        {
            public string ModuleId { get; set; }
            public List<string> SubjectIds { get; set; }
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

        public class InvalidSubjectItem
        {
            public ObjectId SubjectId { get; set; }
            public List<Level> MissingLevels { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<InvalidSubjectItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<InvalidSubjectItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    List<Level> currentLevels = Level.GetLevels().Data;
                    List<InvalidSubjectItem> invalidSubjects = new List<InvalidSubjectItem>();
                    foreach (string subjectId in request.SubjectIds)
                    {
                        ObjectId parsedSubjectId = ObjectId.Parse(subjectId);
                        FilterDefinition<QuestionItem> filters = SetFilters(request.ModuleId, subjectId);
                        var modulesCollection = _db.Database.GetCollection<QuestionItem>("Questions");

                        var qry = await modulesCollection.FindAsync(filters,
                            options: null,
                            cancellationToken: cancellationToken
                        );
                        var questionsList = await qry.ToListAsync();
                        for (int i = 0; i < currentLevels.Count; i++)
                        {
                            if (!questionsList.Any(x => x.Level == currentLevels[i].Id))
                            {
                                var invalidSubject = invalidSubjects.Find(x => x.SubjectId == parsedSubjectId);
                                if (invalidSubject == null)
                                {
                                    invalidSubject = new InvalidSubjectItem
                                    {
                                        SubjectId = parsedSubjectId,
                                        MissingLevels = new List<Level>()
                                    };
                                    invalidSubject.MissingLevels.Add(currentLevels[i]);
                                    invalidSubjects.Add(invalidSubject);
                                }
                                else
                                {
                                    invalidSubject.MissingLevels.Add(currentLevels[i]);
                                }
                            }
                        }
                    }

                    return Result.Ok(invalidSubjects);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<InvalidSubjectItem>>($"Ocorreu um erro ao buscar as questões: {err.Message}");
                }

            }

            private FilterDefinition<QuestionItem> SetFilters(string moduleId, string subjectId)
            {
                var filters = FilterDefinition<QuestionItem>.Empty;
                var builder = Builders<QuestionItem>.Filter;
                    filters = filters & builder.Eq("subjectId", ObjectId.Parse(subjectId));
                    filters = filters & builder.Eq("moduleId", ObjectId.Parse(moduleId));

                return filters;
            }
        }
    }
}

