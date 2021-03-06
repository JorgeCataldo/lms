﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.Enumerations;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.ValuationTests.ValuationTest;

namespace Domain.Aggregates.ValuationTests.Queries
{
    public class GetModuleValuationTests
    {
        public class Contract : CommandContract<Result<List<ValuationTestItem>>>
        {
            public string UserRole { get; set; }
            public string UserId { get; set; }
            public string ModuleId { get; set; }
        }

        public class ValuationTestItem
        {
            public ObjectId Id { get; set; }
            public ValuationTestTypeEnum Type { get; set; }
            public string Title { get; set; }
            public List<TestModule> TestModules { get; set; }
            public bool Answered { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ValuationTestItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ValuationTestItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole != "Student" && request.UserRole != "BusinessManager" && request.UserRole != "Admin")
                    return Result.Ok(new List<ValuationTestItem>());

                var moduleId = ObjectId.Parse(request.ModuleId);
                var userId = ObjectId.Parse(request.UserId);
                var testList = new List<ValuationTestItem>();

                var moduleTests = await _db.ValuationTestCollection
                    .AsQueryable()
                    .Where(x =>
                        x.TestModules.Any(y => y.Id == moduleId)
                    ).ToListAsync();

                if (moduleTests.Count == 0)
                    return Result.Ok(testList);

                var moduleTestIds = moduleTests.Select(x => x.Id);

                var moduleTestResponses = await _db.ValuationTestResponseCollection
                    .AsQueryable()
                    .Where(x => 
                        moduleTestIds.Contains(x.TestId) &&
                        x.CreatedBy == userId
                    )
                    .ToListAsync();

                foreach(ValuationTest test in moduleTests)
                {
                    var newValutaionTestItem = new ValuationTestItem
                    {
                        Id = test.Id,
                        Type = test.Type,
                        Title = test.Title,
                        TestModules = test.TestModules
                    };

                    if (moduleTestResponses.Any(x => x.TestId == test.Id))
                    {
                        newValutaionTestItem.Answered = true;
                    }
                    else
                    {
                        newValutaionTestItem.Answered = false;
                    }
                    testList.Add(newValutaionTestItem);
                }

                return Result.Ok(testList);
            }
        }
    }
}
