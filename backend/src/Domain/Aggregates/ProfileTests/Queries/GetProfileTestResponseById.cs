using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.SuggestedProduct;

namespace Domain.Aggregates.ProfileTests.Queries
{
    public class GetProfileTestResponseById
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
            public bool Recommended { get; set; }
            public List<ProgressInfoItem> ModulesInfo { get; set; }
            public List<ProgressInfoItem> TracksInfo { get; set; }
            public List<ProgressInfoItem> EventsInfo { get; set; }
            public List<SuggestedProductItem> Suggestions { get; set; }
        }

        public class ResponseAnswerItem
        {
            public ObjectId QuestionId { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
            public int? Percentage { get; set; }
            public decimal? Grade { get; set; }
        }

        public class ProgressInfoItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class SuggestedProductItem
        {
            public ObjectId ProductId { get; set; }
            public ObjectId UserId { get; set; }
            public SuggestedProductType Type { get; set; }
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
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail<ResponseItem>("Acesso Negado");

                if (String.IsNullOrEmpty(request.ResponseId))
                    return Result.Fail<ResponseItem>("Id não informado");

                var responseId = ObjectId.Parse(request.ResponseId);

                var response = await _db.Database
                    .GetCollection<ResponseItem>("ProfileTestResponses")
                    .AsQueryable()
                    .Where(r => r.Id == responseId)
                    .FirstOrDefaultAsync();

                if (response == null || response.Answers == null || response.Answers.Count == 0)
                    return Result.Fail<ResponseItem>("Resposta não existe");

                if (response.CreatedBy == null || response.CreatedBy == ObjectId.Empty)
                    return Result.Fail<ResponseItem>("Id de usuário não encontrado");

                response = await GetUserInfo(response, cancellationToken);

                if (response == null)
                    return Result.Fail<ResponseItem>("Usuário não encontrado");

                var questionsIds = response.Answers.Select(a => a.QuestionId);
                
                var questions = await _db.Database
                    .GetCollection<ProfileTestQuestion>("ProfileTestQuestions")
                    .AsQueryable()
                    .Where(tQ => questionsIds.Contains(tQ.Id))
                    .ToListAsync();

                if (questions.Count > 0)
                {
                    foreach (var answer in response.Answers)
                    {
                        var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                        if (question != null)
                            answer.Percentage = question.Percentage;
                    }
                }

                response.Suggestions = await _db.Database
                    .GetCollection<SuggestedProductItem>("SuggestedProducts")
                    .AsQueryable()
                    .Where(s => s.UserId == response.CreatedBy)
                    .ToListAsync(cancellationToken);

                return Result.Ok(response);
            }

            private async Task<ResponseItem> GetUserInfo(ResponseItem response, CancellationToken token)
            {
                var user = await _db.Database
                    .GetCollection<User>("Users")
                    .AsQueryable()
                    .Where(x => x.Id == response.CreatedBy)
                    .FirstOrDefaultAsync(token);

                if (user == null)
                    return null;

                if (user.TracksInfo != null)
                {
                    response.TracksInfo = user.TracksInfo
                        .Where(x => x.Blocked != true)
                        .Select(x => new ProgressInfoItem {
                            Id = x.Id,
                            Name = x.Name
                        })
                        .ToList();
                }

                if (user.ModulesInfo != null)
                {
                    response.ModulesInfo = user.ModulesInfo
                        .Where(x => x.Blocked != true && x.Name != "Requirement")
                        .Select(x => new ProgressInfoItem {
                            Id = x.Id,
                            Name = x.Name
                        })
                        .ToList();
                }

                if (user.EventsInfo != null)
                {
                    response.EventsInfo = user.EventsInfo
                        .Where(x => x.Blocked != true)
                        .Select(x => new ProgressInfoItem {
                            Id = x.Id,
                            Name = x.Name
                        })
                        .ToList();
                }

                return response;
            }
        }
    }
}
