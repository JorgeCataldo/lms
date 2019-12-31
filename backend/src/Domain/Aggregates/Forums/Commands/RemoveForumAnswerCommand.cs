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
    public class RemoveForumAnswerCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string AnswerId { get; set; }
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
                    if (String.IsNullOrEmpty(request.AnswerId))
                        return Result.Fail("Id da resposta não informado");

                    if (request.UserRole == "Student")
                    {
                        ObjectId userId = ObjectId.Parse(request.UserId);
                        ObjectId moduleId = ObjectId.Parse(request.ModuleId);

                        bool isInstructor = await CheckModuleInstructor(moduleId, userId, cancellationToken);
                        if (!isInstructor)
                            return Result.Fail<bool>("Acesso Negado");
                    }

                    var answer = await GetAnswer(request.AnswerId, cancellationToken);
                    if (answer == null)
                        return Result.Fail("Resposta não existe");

                    await _db.ForumAnswerCollection.DeleteOneAsync(fA =>
                        fA.Id == answer.Id,
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

            private async Task<ForumAnswer> GetAnswer(string rAnswerId, CancellationToken cancellationToken)
            {
                var answerId = ObjectId.Parse(rAnswerId);

                var query = await _db.Database
                    .GetCollection<ForumAnswer>("ForumAnswers")
                    .FindAsync(
                        x => x.Id == answerId,
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
