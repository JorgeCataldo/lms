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
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Forums.Queries
{
    public class GetContactAreasQuery
    {
        public class Contract : CommandContract<Result<List<ContactAreaItem>>> { } 

        public class ContactAreaItem
        {
            public ObjectId Id { get; set; }
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<List<ContactAreaItem>>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
            }

            public async Task<Result<List<ContactAreaItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var query = await _db.Database
                        .GetCollection<ContactAreaItem>("ContactAreas")
                        .FindAsync(FilterDefinition<ContactAreaItem>.Empty);

                    var areas = await query.ToListAsync();

                    return Result.Ok(areas);
                }
                catch (Exception err)
                {
                    return Result.Fail<List<ContactAreaItem>>(err.Message);
                }
            }
        }
    }
}
