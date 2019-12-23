using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Responsibles.Commands
{
    public class GenerateResponsibleTreeCommand
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string UserRole { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public ObjectId ResponsibleId { get; set; }
        }

        public class Handler : IRequestHandler<Contract,
            Result<bool>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<bool>("Acesso Negado");

                var users = _db.UserCollection
                    .AsQueryable()
                    .Where(x => !x.IsBlocked)
                    .Select(x => new UserItem { Id = x.Id, ResponsibleId = x.ResponsibleId })
                    .ToList();

                var responsibleList = new List<Responsible>();

                foreach (UserItem user in users)
                {
                    if (HasResponsible(user))
                    {
                        var responsible = responsibleList.Find(x => x.ResponsibleUserId == user.ResponsibleId);

                        if (responsible == null)
                        {
                            var studentUserIdList = new List<ObjectId>
                            {
                                user.Id
                            };

                            var responsibleResult = Responsible.Create(user.ResponsibleId, studentUserIdList);

                            if (responsibleResult.IsSuccess)
                                responsibleList.Add(responsibleResult.Data);
                        }
                        else
                        {
                            responsible.SubordinatesUsersIds.Add(user.Id);
                        }
                    }
                }

                foreach (Responsible responsible in responsibleList)
                {
                    var responsibleFathers = responsibleList.Where(x => x.ResponsibleUserId != responsible.ResponsibleUserId && 
                        x.SubordinatesUsersIds.Contains(responsible.ResponsibleUserId)).ToList();
                    if (responsibleFathers.Count > 0)
                    {
                        foreach (Responsible responsibleFather in responsibleFathers)
                        {
                            foreach (ObjectId subordinateUserId in responsible.SubordinatesUsersIds)
                            {
                                if (!responsibleFather.SubordinatesUsersIds.Contains(subordinateUserId))
                                    responsibleFather.SubordinatesUsersIds.Add(subordinateUserId);
                            }
                        }
                    }
                }

                await _db.ResponsibleCollection.DeleteManyAsync(x => x.ResponsibleUserId != null, cancellationToken);
                await _db.ResponsibleCollection.InsertManyAsync(responsibleList, cancellationToken: cancellationToken);

                return Result.Ok(true);
            }

            private bool HasResponsible(UserItem user) {
                if (user.ResponsibleId == null || user.ResponsibleId == ObjectId.Empty)
                    return false;
                return true;
            }
        }
    }
}
