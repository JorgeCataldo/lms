using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Domain.Aggregates.Users.Commands
{
    public abstract class BaseAccountCommand
    {
        protected readonly IDbContext Context;

        protected BaseAccountCommand(IDbContext context )
        {
            Context = context;
        }
        
        protected async Task<Guid> SaveRefreshToken(ObjectId userId, Guid refreshToken, CancellationToken cancellationToken)
        {
            var filter = Builders<User>.Filter.Eq("_id", userId);
            var update = Builders<User>.Update.Set("refreshToken", refreshToken);
            await Context.UserCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return refreshToken;
        }
    }
}