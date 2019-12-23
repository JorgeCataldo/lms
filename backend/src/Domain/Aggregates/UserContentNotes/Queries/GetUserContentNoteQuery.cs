using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserContentNotes.Queries
{
    public class GetUserContentNoteQuery
    {
        public class Contract : CommandContract<Result<UserContentNote>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<UserContentNote>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<UserContentNote>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<UserContentNote>("O campo moduleId é obrigatório");
                if (String.IsNullOrEmpty(request.SubjectId))
                    return Result.Fail<UserContentNote>("O campo subjectId é obrigatório");
                if (String.IsNullOrEmpty(request.ContentId))
                    return Result.Fail<UserContentNote>("O campo contentId é obrigatório");

                var userId = ObjectId.Parse(request.UserId);
                var moduleId = ObjectId.Parse(request.ModuleId);
                var subjectId = ObjectId.Parse(request.SubjectId);
                var contentId = ObjectId.Parse(request.ContentId);

                var baseContentNote = await _db.UserContentNoteCollection.AsQueryable()
                .Where(x => x.UserId == userId &&
                    x.ModuleId == moduleId &&
                    x.SubjectId == subjectId &&
                    x.ContentId == contentId)
                .FirstOrDefaultAsync();

                if (baseContentNote == null)
                    return Result.Ok(new UserContentNote());

                return Result.Ok(baseContentNote);
            }
        }
    }
}
