using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Commands;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.VerificationEmail.Commands
{
    public class CheckVerificationEmail
    {
        public class Contract: CommandContract<Result<TokenInfo>>
        {
            [Required]
            public string UserId { get; set; }
            public string Code { get; set; }

        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<TokenInfo>>
        {
            private readonly IDbContext _db;
            private readonly ITokenGenerator _tokenGenerator;

            public Handler(IDbContext context, 
                IDbContext db,
                ITokenGenerator tokenGenerator) : base(context)
            {
                _db = db;
                _tokenGenerator = tokenGenerator;
            }
            
            public async Task<Result<TokenInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userId = ObjectId.Parse(request.UserId);

                var user = await GetUser(userId, cancellationToken);
                if (user == null)
                    return Result.Fail<TokenInfo>("Usuário não encontrado");

                var verificationEmail = _db.UserVerificationEmailCollection.AsQueryable()
                        .Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.CreatedAt)
                        .FirstOrDefault();
                if (verificationEmail == null)
                    return Result.Fail<TokenInfo>("Código não encontrado");

                if (verificationEmail.Code != request.Code)
                    return Result.Fail<TokenInfo>("Código esta incorreto");

                user.EmailVerified = true;
                await _db.UserCollection.ReplaceOneAsync(t =>
                    t.Id == user.Id, user,
                    cancellationToken: cancellationToken
                );

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);

                return Result.Ok(tokenResource);
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                var userQuery = await _db.UserCollection.FindAsync(u =>
                    u.Id == userId,
                    cancellationToken: token
                );

                return await userQuery.SingleOrDefaultAsync(token);
            }
        }
    }
}