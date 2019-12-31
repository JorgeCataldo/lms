using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class FirstAccess
    {
        public class Contract: CommandContract<Result<TokenInfo>>
        {
            [Required]
            public string Username { get; set; }
            [Required]
            public string FullName { get; set; }
            [Required]
            public string Phone { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Cpf { get; set; }

        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<TokenInfo>>
        {
            private readonly UserManager<User> _userManager;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly IDbContext _db;

            public Handler(
                IDbContext context,
                UserManager<User> userManager,
                ITokenGenerator tokenGenerator,
                IDbContext db) : base(context)
            {
                _userManager = userManager;
                _tokenGenerator = tokenGenerator;
                _db = db;
            }
            
            public async Task<Result<TokenInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByNameAsync(request.Username);
            
                if (user == null || user.IsBlocked)
                    return Result.Fail<TokenInfo>("Usuário ou Senha inválido");

                user.Name = request.FullName;
                user.Phone = request.Phone;
                user.Email = request.Email;
                user.Cpf = request.Cpf;
                user.FirstAccess = false;

                await _db.UserCollection.ReplaceOneAsync(x => x.Id == user.Id, user,
                    cancellationToken: cancellationToken
                );

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);
                
                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);

                return Result.Ok(tokenResource);
            }
        }
    }
}