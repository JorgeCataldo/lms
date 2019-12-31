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

namespace Domain.Aggregates.UserFiles.Commands
{
    public class GetUserFilesQuery
    {
        public class Contract : CommandContract<Result<List<UserFile>>>
        {
            public string FilesUserId { get; set; }
            public ObjectId CurrentUserId { get; set; }
            public string CurrentUserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<UserFile>>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<List<UserFile>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.CurrentUserRole == "Secretaria")
                    return Result.Fail<List<UserFile>>("Acesso Negado");

                var filesUserId = ObjectId.Parse(request.FilesUserId);

                if (
                    request.CurrentUserRole == "Student" &&
                    filesUserId != request.CurrentUserId
                ) {
                    return Result.Fail<List<UserFile>>("Acesso Negado");
                }
                
                var userFiles = await _db.UserFileCollection.AsQueryable()
                    .Where(t => t.CreatedBy == filesUserId)
                    .ToListAsync();

                return Result.Ok( userFiles );
            }
        }
    }
}
