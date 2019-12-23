using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.ModulesDrafts;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.QuestionsDraft.Commands
{
    public class ManageQuestionDraft
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string QuestionId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string Id { get; set; }
            public string Text { get; set; }
            public List<ContractAnswer> Answers { get; set; }
            public string[] Concepts { get; set; }
            public int Level { get; set; }
            public int Duration { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }

            public Contract()
            {
                Answers = new List<ContractAnswer>();
            }
        }

        public class ContractAnswer
        {
            public string Description { get; set; }
            public int Points { get; set; }
            public List<ContractAnswerConcept> Concepts { get; set; }

            public ContractAnswer()
            {
                Concepts = new List<ContractAnswerConcept>();
            }
        }

        public class ContractAnswerConcept
        {
            public string Concept { get; set; }
            public bool IsRight { get; set; }
        }


        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary" || request.UserRole == "Recruiter")
                    return Result.Fail<Contract>("Acesso Negado");

                var mId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);

                var module = await _db.ModuleDraftCollection.AsQueryable()
                    .Where(x => (
                            x.DeletedAt == null || x.DeletedAt == DateTimeOffset.MinValue
                        ) && !x.DraftPublished && (
                            x.Id == mId || x.ModuleId == mId
                        )
                    )
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                    return Result.Fail<Contract>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    if (!ModuleDraft.IsInstructor(module, userId).Data)
                        return Result.Fail<Contract>("Acesso Negado");
                }

                var subject = module.Subjects.FirstOrDefault(x =>
                    x.Id == ObjectId.Parse(request.SubjectId)
                );

                if (subject == null)
                    return Result.Fail<Contract>("Assunto não Encontrado");

                var answers = request.Answers.Select(x =>
                    new Answer(
                        x.Description, x.Points,
                        x.Concepts.Select(y => new AnswerConcept(y.Concept, y.IsRight)).ToList()
                    )
                ).ToList();

                Result<QuestionDraft> newObject;

                if (!String.IsNullOrEmpty(request.QuestionId))
                {
                    var questionId = ObjectId.Parse(request.QuestionId);

                    newObject = QuestionDraft.Create(
                        questionId, module.Id, request.Text, request.Level, request.Duration,
                        request.Concepts, answers, module.ModuleId, ObjectId.Parse(request.SubjectId)
                    );
                }
                else
                {
                    newObject = QuestionDraft.Create(
                        module.Id, request.Text, request.Level, request.Duration,
                        request.Concepts, answers, module.ModuleId, ObjectId.Parse(request.SubjectId)
                    );
                }

                if (newObject.IsFailure)
                    return Result.Fail<Contract>(newObject.Error);

                var newQuestion = newObject.Data;
                var newQuestionList = new List<QuestionDraft>
                {
                    newQuestion
                };


                if (string.IsNullOrEmpty(request.Id))
                {
                    await _db.QuestionDraftCollection.InsertOneAsync(
                        newQuestion,
                        cancellationToken: cancellationToken
                    );

                    request.Id = newQuestion.Id.ToString();

                    var creationLog = AuditLog.Create(userId, mId, newQuestionList[0].GetType().ToString(),
                    JsonConvert.SerializeObject(newQuestionList), EntityAction.Add);

                    await _db.AuditLogCollection.InsertOneAsync(creationLog);
                }
                else
                {
                    var question = await _db.QuestionDraftCollection
                        .AsQueryable()
                        .Where(x => x.Id == ObjectId.Parse(request.Id))
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (question == null)
                        return Result.Fail<Contract>("Questão não Encontrada");

                    var oldQuestionList = new List<QuestionDraft>
                    {
                        question
                    };
                    var oldValues = JsonConvert.SerializeObject(oldQuestionList);

                    question.ModuleId = newQuestion.ModuleId;
                    question.SubjectId = newQuestion.SubjectId;
                    question.Concepts = newQuestion.Concepts;
                    question.Answers = newQuestion.Answers;
                    question.Duration = newQuestion.Duration;
                    question.Level = newQuestion.Level;
                    question.Text = newQuestion.Text;
                    question.UpdatedAt = DateTimeOffset.UtcNow;

                    await _db.QuestionDraftCollection.ReplaceOneAsync(t =>
                        t.Id == question.Id, question,
                        cancellationToken: cancellationToken
                    );

                    var changeLog = AuditLog.Create(userId, mId, newQuestionList[0].GetType().ToString(),
                    JsonConvert.SerializeObject(newQuestionList), EntityAction.Update, oldValues);

                    await _db.AuditLogCollection.InsertOneAsync(changeLog);
                }

                return Result.Ok(request);
            }
        }
    }
}
