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

namespace Domain.Aggregates.Questions.Commands
{
    public class AddOrModifyQuestionCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
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
                if (request.UserRole == "Secretary")
                    return Result.Fail<Contract>("Acesso Negado");

                var mId = ObjectId.Parse(request.ModuleId);
                var module = await (await _db
                    .Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x => x.Id == mId, cancellationToken: cancellationToken))
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (module == null)
                    return Result.Fail<Contract>("Módulo não Encontrado");

                if (request.UserRole == "Student")
                {
                    var userId = ObjectId.Parse(request.UserId);

                    if (!Module.IsInstructor(module, userId).Data)
                        return Result.Fail<Contract>("Acesso Negado");
                }

                var subject = module.Subjects.FirstOrDefault(x => x.Id == ObjectId.Parse(request.SubjectId));
                
                if (subject == null)
                    return Result.Fail<Contract>("Assunto não Encontrado");

                var answers = request.Answers.Select(x => new Answer(x.Description, x.Points,
                    x.Concepts.Select(y => new AnswerConcept(y.Concept, y.IsRight)).ToList())).ToList();
                
                var newObject = Question.Create(request.Text, request.Level, request.Duration, request.Concepts,
                    answers,
                    mId, ObjectId.Parse(request.SubjectId));

                if (newObject.IsFailure)
                    return Result.Fail<Contract>(newObject.Error);

                var newQuestion = newObject.Data;

                if (string.IsNullOrEmpty(request.Id))
                {
                    await _db.QuestionCollection.InsertOneAsync(newObject.Data, cancellationToken: cancellationToken);
                    request.Id = newQuestion.Id.ToString();
                }
                else
                {
                    var question = await (await _db
                            .Database
                            .GetCollection<Question>("Questions")
                            .FindAsync(x => x.Id == ObjectId.Parse(request.Id), cancellationToken: cancellationToken))
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    
                    if (question == null)
                    {
                        return Result.Fail<Contract>("Questão não Encontrada");
                    }
                    
                    question.ModuleId = newObject.Data.ModuleId;
                    question.SubjectId = newObject.Data.SubjectId;
                    question.Concepts = newObject.Data.Concepts;
                    question.Answers = newObject.Data.Answers;
                    question.Duration = newObject.Data.Duration;
                    question.Level = newObject.Data.Level;
                    question.Text = newObject.Data.Text;
                    question.UpdatedAt = DateTimeOffset.UtcNow;

                    await _db.QuestionCollection.ReplaceOneAsync(t => t.Id == question.Id, question, cancellationToken: cancellationToken);
                }

                return Result.Ok(request);
            }
        }
    }
}
