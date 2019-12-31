using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ProfileTests.ProfileTestResponse;
using static Domain.Aggregates.ProfileTests.SuggestedProduct;

namespace Domain.Aggregates.ProfileTests.Commands
{
    public class SuggestProfileTestCommand
    {
        public class Contract : CommandContract<Result>
        {
            public List<string> UsersIds { get; set; }
            public string TestId { get; set; }
            public string CurrentUserRole { get; set; }
        }

        public class SuggestedProductItem
        {
            public string ProductId { get; set; }
            public SuggestedProductType Type { get; set; }
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
                if (request.CurrentUserRole == "Student" || request.CurrentUserRole == "Recruiter")
                    return Result.Fail("Acesso Negado");

                if (String.IsNullOrEmpty(request.TestId))
                    return Result.Fail("Acesso Negado");

                if (request.UsersIds == null || request.UsersIds.Count == 0)
                    return Result.Fail("É necessário selecionar pelo menos um usuário para indicar testes");

                var test = await GetProfileTest(request, cancellationToken);
                if (test == null)
                    return Result.Fail<Contract>("Teste não existe");
                
                var users = await GetUsers(request.UsersIds, cancellationToken);

                var responses = new List<ProfileTestResponse>();

                foreach (var user in users)
                {
                    var newResponse = Create(
                        test.Id, test.Title,
                        user.Id, user.Name, user.RegistrationId,
                        new List<ProfileTestAnswer>()
                    ).Data;

                    responses.Add(newResponse);
                }

                await _db.ProfileTestResponseCollection.InsertManyAsync(
                    responses, cancellationToken: cancellationToken
                );

                return Result.Ok();
            }

            public async Task<ProfileTest> GetProfileTest(Contract request, CancellationToken token)
            {
                var testId = ObjectId.Parse(request.TestId);

                return await _db.ProfileTestCollection.AsQueryable()
                    .Where(t => t.Id == testId)
                    .FirstOrDefaultAsync(token);
            }

            public async Task<List<User>> GetUsers(List<string> usersIds, CancellationToken token)
            {
                var objectIds = usersIds.Select(uI => ObjectId.Parse(uI));
                var query = _db.UserCollection.AsQueryable()
                    .Where(t => objectIds.Contains(t.Id));
                return await ((IAsyncCursorSource<User>)query).ToListAsync(token);
            }
        }
    }
}
