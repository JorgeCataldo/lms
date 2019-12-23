using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTestQuestion;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetValuationTestResponseById
    {
        public class Contract : CommandContract<Result<ResponseItem>>
        {
            public string UserRole { get; set; }
            public string ResponseId { get; set; }
        }

        public class ResponseItem
        {
            public ObjectId Id { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public ObjectId CreatedBy { get; set; }
            public string UserName { get; set; }
            public string UserRegisterId { get; set; }
            public ObjectId TestId { get; set; }
            public string TestTitle { get; set; }
            public List<ResponseAnswerItem> Answers { get; set; }
        }

        public class ResponseAnswerItem
        {
            public ObjectId QuestionId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public int? Percentage { get; set; }
            public decimal? Grade { get; set; }
            public ValuationTestQuestionType? Type { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ResponseItem>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<ResponseItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Fail<ResponseItem>("Acesso Negado");

                if (String.IsNullOrEmpty(request.ResponseId))
                    return Result.Fail<ResponseItem>("Id não informado");

                var responseId = ObjectId.Parse(request.ResponseId);

                var response = await _db.Database
                    .GetCollection<ResponseItem>("ValuationTestResponses")
                    .AsQueryable()
                    .Where(r => r.Id == responseId)
                    .FirstOrDefaultAsync();

                if (response == null || response.Answers == null || response.Answers.Count == 0)
                    return Result.Fail<ResponseItem>("Resposta não existe");

                if (response.CreatedBy == null || response.CreatedBy == ObjectId.Empty)
                    return Result.Fail<ResponseItem>("Id de usuário não encontrado");

                return Result.Ok(response);
            }
        }
    }
}
