using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class ForgotPassword
    {
        public class Contract: CommandContract<Result>
        {
            public string Username { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result>
        {
            private readonly IDbContext _db;
            private readonly IEmailProvider _provider;
            private readonly UserManager<User> _userManager;
            private readonly IConfiguration _configuration;
            private readonly ITokenGenerator _tokenGenerator;

            public Handler(IDbContext db, IEmailProvider provider, IConfiguration configuration, UserManager<User> userManager, ITokenGenerator tokenGenerator) : base(db)
            {
                _db = db;
                _provider = provider;
                _configuration = configuration;
                _userManager = userManager;
                _tokenGenerator = tokenGenerator;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                string siteUrl = _configuration[$"DomainOptions:SiteUrl"];
                var userQry = await _db.UserCollection.FindAsync(u => u.UserName == request.Username, cancellationToken: cancellationToken);
                var user = await userQry.SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    return Result.Fail("Usuário não encontrado");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var randomPassword = Guid.NewGuid().ToString("d").Substring(1, 6);
                var pass = await _userManager.ResetPasswordAsync(user, token, randomPassword);

                if(!pass.Succeeded)
                    return Result.Fail("Não foi possivel resetar a senha");

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);

                var emailData = new EmailUserData
                {
                    Email = user.Email,
                    Name = user.Name,
                    ExtraData = new Dictionary<string, string>{
                        { "name", user.UserName },
                        { "token", randomPassword },
                        { "siteurl", siteUrl }
                    }
                };

                await _provider.SendEmail(emailData, "Esqueci minha senha", "BTG-ForgotPasswordTemplate");   
                
                return Result.Ok();
            }
        }
    }
}