using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Commands
{
    public class RemoveForumQuestionCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string QuestionId { get; set; }
            public string ModuleId { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.QuestionId))
                        return Result.Fail("Id da pergunta não informado");

                    if (request.UserRole == "Student")
                    {
                        ObjectId userId = ObjectId.Parse(request.UserId);
                        ObjectId moduleId = ObjectId.Parse(request.ModuleId);

                        bool isInstructor = await CheckModuleInstructor(moduleId, userId, cancellationToken);
                        if (!isInstructor)
                            return Result.Fail<bool>("Acesso Negado");
                    }

                    var question = await GetQuestion(request.QuestionId, cancellationToken);
                    if (question == null)
                        return Result.Fail("Pergunta não existe");

                    await _db.ForumQuestionCollection.DeleteOneAsync(fQ =>
                        fQ.Id == question.Id,
                        cancellationToken: cancellationToken
                    );

                    await _db.NotificationCollection.DeleteManyAsync(notification =>
                        notification.RedirectPath.Contains(
                            question.Id.ToString()
                        ),
                        cancellationToken: cancellationToken
                    );

                    return Result.Ok();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw err;
                }
            }

            private async Task<ForumQuestion> GetQuestion(string rQuestionId, CancellationToken cancellationToken)
            {
                var questionId = ObjectId.Parse(rQuestionId);

                var query = await _db.Database
                    .GetCollection<ForumQuestion>("ForumQuestions")
                    .FindAsync(
                        x => x.Id == questionId,
                        cancellationToken: cancellationToken
                    );

                return await query.FirstOrDefaultAsync();
            }

            private async Task<bool> CheckModuleInstructor(
                ObjectId moduleId, ObjectId userId, CancellationToken cancellationToken
            ) {
                var query = await _db.Database
                    .GetCollection<Module>("Modules")
                    .FindAsync(x =>
                        x.Id == moduleId &&
                        (
                            (x.InstructorId.HasValue && x.InstructorId.Value == userId) ||
                            x.ExtraInstructorIds.Contains(userId)
                        ),
                        cancellationToken: cancellationToken
                    );
                return await query.AnyAsync();
            }
        }
    }
}
