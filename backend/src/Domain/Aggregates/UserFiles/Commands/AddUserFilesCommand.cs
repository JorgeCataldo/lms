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
    public class AddUserFilesCommand
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
                try
                {
                    if (request.CurrentUserRole == "Secretaria")
                        return Result.Fail<Contract>("Acesso Negado");

                    var filesUserId = ObjectId.GenerateNewId();

                    if (
                        request.CurrentUserRole == "Student" &&
                        filesUserId != request.CurrentUserId
                    )
                    {
                        return Result.Fail<Contract>("Acesso Negado");
                    }

                    if (request.UserFiles.Count > 10)
                        return Result.Fail<Contract>("O limite de envios é de 10 arquivos");

                    var userFiles = await _db.UserFileCollection.AsQueryable()
                        .Where(t => t.CreatedBy == filesUserId)
                        .ToListAsync();

                    var currentFilesIds = request.UserFiles
                        .Where(uf => !string.IsNullOrEmpty(uf.Id))
                        .Select(uf => ObjectId.Parse(uf.Id));

                    await _db.UserFileCollection.DeleteManyAsync(f =>
                        !currentFilesIds.Contains(f.Id),
                        cancellationToken: cancellationToken
                    );

                    foreach (var userFile in request.UserFiles)
                    {
                        if (!string.IsNullOrEmpty(userFile.Id))
                        {
                            var userFileId = ObjectId.Parse(userFile.Id);
                            var dbFile = userFiles.FirstOrDefault(f =>
                                f.Id == userFileId
                            );

                            if (dbFile != null)
                            {
                                dbFile.Title = userFile.Title;
                                dbFile.Description = userFile.Description;
                                dbFile.DownloadLink = userFile.DownloadLink;
                                dbFile.Resource = userFile.Resource;

                                await _db.UserFileCollection.ReplaceOneAsync(t =>
                                    t.Id == dbFile.Id, dbFile,
                                    cancellationToken: cancellationToken
                                );
                            }
                            else
                                return Result.Fail<Contract>("Arquivo editado não existe");
                        }
                        else
                        {
                            var newFile = UserFile.Create(
                                filesUserId,
                                userFile.Title,
                                userFile.Description,
                                userFile.DownloadLink,
                                userFile.Resource,
                                ObjectId.GenerateNewId().ToString()
                            ).Data;

                            await _db.UserFileCollection.InsertOneAsync(
                                newFile,
                                cancellationToken: cancellationToken
                            );
                        }
                    }

                    return Result.Ok(request);
                }
                catch(Exception ex)
                {
                    return Result.Fail<Contract>(ex.Message);
                }
            }
        }
    }
}
