using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using MediatR;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class RefreshToken
    {
        public class Contract: CommandContract<Result<TokenInfo>>
        {
            public string RefreshToken { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<TokenInfo>>
        {
            private readonly ITokenGenerator _tokenGenerator;

            public Handler(                
                IDbContext context,
                ITokenGenerator tokenGenerator): base(context)
            {
                _tokenGenerator = tokenGenerator;
            }
            
            public async Task<Result<TokenInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var guid = Guid.Parse(request.RefreshToken);                
                var existingUsers = await Context.UserCollection
                    .FindAsync(x => x.RefreshToken == guid && x.DeletedAt != null, cancellationToken: cancellationToken);
                var user = existingUsers.FirstOrDefault();
            
                if (user == null || user.IsBlocked)
                    return Result.Fail<TokenInfo>("Não foi possível renovar o login");
                
                var tokenResource = await _tokenGenerator.GenerateUserToken(user);             
                
                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken); 

                return Result.Ok(tokenResource);
            }
        }
    }
}