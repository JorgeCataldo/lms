using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.Enumerations;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules.Commands
{
    public class SaveActionCommand
    {
        public class Contract : CommandContract<Result<Action>>
        {
            public int PageId { get; set; }
            public string Description { get; set; }
            public int TypeId { get; set; }
            public string ModuleId { get; set; }
            public string EventId { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
            public string Concept { get; set; }
            public string SupportMaterialId { get; set; }
            public string UserId { get; set; }
            public string QuestionId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Action>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<Action>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var pageResult = ActionPage.Create(request.PageId);
                    if (pageResult.IsFailure)
                        return Result.Fail<Action>(pageResult.Error);

                    var typeResult = ActionType.Create(request.TypeId);
                    if (typeResult.IsFailure)
                        return Result.Fail<Action>(typeResult.Error);

                    var action = Action.Create(
                        pageResult.Data, request.Description, typeResult.Data, request.UserId,
                        request.ModuleId, request.EventId,
                        request.SubjectId, request.ContentId,
                        request.Concept, request.SupportMaterialId, request.QuestionId
                    );

                    if (action.IsFailure)
                        return Result.Fail<Action>(action.Error);

                    await _db.ActionCollection.InsertOneAsync(
                        action.Data, cancellationToken: cancellationToken
                    );

                    return action;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw err;
                }
            }

            /*private async Task<bool> IsModuleFirstAcess(Action action)
            {
                if (action.Type == ActionType.Access && !string.IsNullOrEmpty(action.ModuleId))
                {
                    var access = await _db.ActionCollection.AsQueryable()
                        .FirstOrDefaultAsync(x =>
                            x.CreatedBy == action.CreatedBy &&
                            x.Type == action.Type && 
                            x.ModuleId == action.ModuleId
                        );

                    if (access != null)
                    {
                        return false;
                    }

                    var moduleId = ObjectId.Parse(action.ModuleId);

                    var user = await _db.UserCollection.AsQueryable()
                        .FirstOrDefaultAsync(x => x.Id == action.CreatedBy);

                    if (user == null)
                        return false;

                    // user.ModulesInfo.Where(x => x.Id == )
                    return true;
                }
                return false;
            }*/
        }
    }
}
