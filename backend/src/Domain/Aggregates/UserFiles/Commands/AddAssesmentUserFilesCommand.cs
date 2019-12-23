using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserFiles.Commands
{
    public class AddAssesmentUserFilesCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string FilesUserId { get; set; }
            public ObjectId CurrentUserId { get; set; }
            public string CurrentUserRole { get; set; }
            public List<UserFileItem> UserFiles { get; set; }
        }

        public class UserFileItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DownloadLink { get; set; }
            public string Resource { get; set; }
            public string ResourceId { get; set; }
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
                if (request.CurrentUserRole == "Secretaria")
                    return Result.Fail<Contract>("Acesso Negado");

                if (request.UserFiles.Count > 10)
                    return Result.Fail<Contract>("O limite de envios é de 10 arquivos");

                foreach (var userFile in request.UserFiles)
                {
                    var newFile = UserFile.Create(
                        request.CurrentUserId,
                        userFile.Title,
                        userFile.Description,
                        userFile.DownloadLink,
                        userFile.Resource,
                        userFile.ResourceId
                    ).Data;

                    await _db.UserFileCollection.InsertOneAsync(
                        newFile,
                        cancellationToken: cancellationToken
                    );

                }

                return Result.Ok(request);
            }
        }
    }
}
