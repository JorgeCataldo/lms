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
using static Domain.Aggregates.ValuationTests.ValuationTestResponse;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetAllValuationTestResponses
    {
        public class Contract : CommandContract<Result<List<ResponseItem>>>
        {
            public string UserRole { get; set; }
            public string TestId { get; set; }
        }

        public class ResponseItem
        {
            public ObjectId Id { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string UserName { get; set; }
            public string UserRegisterId { get; set; }
            public List<ValuationTestAnswer> Answers { get; set; }
        }

        public class AnswerItem
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ResponseItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ResponseItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Fail<List<ResponseItem>>("Acesso Negado");

                if (String.IsNullOrEmpty(request.TestId))
                    return Result.Fail<List<ResponseItem>>("Acesso Negado");
                
                var testId = ObjectId.Parse(request.TestId);

                var collection = _db.ValuationTestResponseCollection.AsQueryable();
                var responses = await collection
                    .Where(r =>
                        r.TestId == testId &&
                        r.Answers != null &&
                        r.Answers.Count > 0
                    )
                    .Select(r => new ResponseItem {
                        Id = r.Id,
                        CreatedAt = r.CreatedAt,
                        UserName = r.UserName,
                        UserRegisterId = r.UserRegisterId,
                        Answers = r.Answers
                    })
                    .ToListAsync();

                return Result.Ok( responses );
            }
        }
    }
}
