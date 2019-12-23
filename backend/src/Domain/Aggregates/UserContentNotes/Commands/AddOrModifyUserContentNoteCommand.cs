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

namespace Domain.Aggregates.UserContentNotes.Commands
{
    public class AddOrModifyUserContentNoteCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string UserId { get; set; }
            public string ModuleId { get; set; }
            public string SubjectId { get; set; }
            public string ContentId { get; set; }
            public string Note { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.ModuleId))
                    return Result.Fail<Contract>("O campo moduleId é obrigatório");
                if (String.IsNullOrEmpty(request.SubjectId))
                    return Result.Fail<Contract>("O campo subjectId é obrigatório");
                if (String.IsNullOrEmpty(request.ContentId))
                    return Result.Fail<Contract>("O campo contentId é obrigatório");

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
                {
                    var userContentNoteResult = UserContentNote.Create(userId, moduleId, subjectId, contentId, request.Note);
                    if (userContentNoteResult.IsFailure)
                        return Result.Fail<Contract>(userContentNoteResult.Error);

                    baseContentNote = userContentNoteResult.Data;
                    await _db.UserContentNoteCollection.InsertOneAsync(baseContentNote);
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.Note) && request.Note.Length > 4000)
                    {
                        return Result.Fail<Contract>("A nota não pode possuir mais de 4000 caracteres");
                    }
                    baseContentNote.Note = request.Note;
                    await _db.UserContentNoteCollection.ReplaceOneAsync(t =>
                        t.Id == baseContentNote.Id, baseContentNote,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(request);
            }
        }
    }
}
