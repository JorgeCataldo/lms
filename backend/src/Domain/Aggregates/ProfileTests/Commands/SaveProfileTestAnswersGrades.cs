using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class SaveProfileTestAnswersGrades
    {
        public class Contract : CommandContract<Result>
        {
            public string Id { get; set; }
            public List<ProfileTestAnswerItem> Answers { get; set; }
            public string UserRole { get; set; }
        }

        public class ProfileTestAnswerItem
        {
            public string QuestionId { get; set; }
            public int Percentage { get; set; }
            public decimal Grade { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Student" || request.UserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.Id))
                    return Result.Fail("Acesso Negado");

                var responseId = ObjectId.Parse(request.Id);

                var response = await _db.ProfileTestResponseCollection.AsQueryable()
                    .Where(r => r.Id == responseId)
                    .FirstOrDefaultAsync();

                if (response == null)
                    return Result.Fail("Resposta de Teste não encontrada");

                foreach (var item in request.Answers)
                {
                    var questioId = ObjectId.Parse(item.QuestionId);

                    var dbAnswer = response.Answers.FirstOrDefault(a =>
                        a.QuestionId == questioId
                    );

                    if (dbAnswer != null && !dbAnswer.Grade.HasValue)
                        dbAnswer.Grade = item.Grade;
                }

                response.UpdatedAt = DateTimeOffset.Now;

                await _db.ProfileTestResponseCollection.ReplaceOneAsync(
                    r => r.Id == response.Id, response,
                    cancellationToken: cancellationToken
                );

                return Result.Ok();
            }
        }
    }
}
