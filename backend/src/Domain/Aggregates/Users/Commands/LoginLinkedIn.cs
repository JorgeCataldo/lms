using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.ExternalService;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class LoginLinkedIn
    {
        public class Contract: CommandContract<Result<LoginItem>>
        {
            [Required]
            public string Code { get; set; }
        }

        public class AccessTokenItem
        {
            public string Access_token { get; set; }
            public string Expires_in { get; set; }
        }

        public class LinkedInUserItem
        {
            public string LinkedInId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        public class LoginItem
        {
            public TokenInfo TokenInfo { get; set; }
            public TokenUser User { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<LoginItem>>
        {
            private readonly IConfiguration _configuration;
            private readonly IDbContext _db;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly UserManager<User> _userManager;

            public Handler(IConfiguration configuration, 
                IDbContext db,
                ITokenGenerator tokenGenerator,
                UserManager<User> userManager) : base(db)
            {
                _userManager = userManager;
                _configuration = configuration;
                _db = db;
                _tokenGenerator = tokenGenerator;
            }
            
            public async Task<Result<LoginItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Code)) 
                    return Result.Fail<LoginItem>("Codigo inválido");

                var response = await GetAccessToken(request.Code);
                string content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.Fail<LoginItem>("Ocorreu um erro ao se comunicar com o LinkedIn");
                
                var parsed = JObject.Parse(content);

                var accessToken = (string)parsed["access_token"];

                var responseDados = await GetDadosBasicos(accessToken);
                string contentDados = await responseDados.Content.ReadAsStringAsync();
                var parsedDados = JObject.Parse(contentDados);


                var responseEmail = await GetEmail(accessToken);
                string contentEmail = await responseEmail.Content.ReadAsStringAsync();
                var parsedEmail = JObject.Parse(contentEmail);

                var linkedInUser = new LinkedInUserItem()
                {
                    LinkedInId = (string)parsedDados["id"],
                    FirstName = (string)parsedDados["firstName"]["localized"]["pt_BR"],
                    LastName = (string)parsedDados["lastName"]["localized"]["pt_BR"],
                    Email = (string)parsedEmail["elements"][0]["handle~"]["emailAddress"]
                };

                var user = await GetUserByLinkedInId(linkedInUser.LinkedInId, cancellationToken);

                if (user != null)
                {
                    return Result.Ok(await GenerateLoginItem(user, cancellationToken));
                }

                user = await GetUserByLinkedInEmail(linkedInUser.Email, cancellationToken);

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.LinkedInId))
                        return Result.Fail<LoginItem>("Este usuário ja esta associado com uma conta do LinkedIn");
                    return Result.Ok(await BindUserLinkedIn(user, linkedInUser, cancellationToken));
                }

                return Result.Ok(await CreateNewUserLinkedIn(linkedInUser, cancellationToken));
            }

            private async Task<HttpResponseMessage> GetAccessToken(string code)
            {
                return await LinkedIn.AccessToken(
                    code, _configuration
                );
            }

            private async Task<HttpResponseMessage> GetDadosBasicos(string code)
            {
                return await LinkedIn.DadosBasicos(
                    code
                );
            }

            private async Task<HttpResponseMessage> GetEmail(string code)
            {
                return await LinkedIn.Email(
                    code
                );
            }

            private async Task<User> GetUserByLinkedInId(string linkedId, CancellationToken token)
            {
                return await _db.UserCollection
                    .AsQueryable()
                    .Where(x => x.LinkedInId == linkedId)
                    .FirstOrDefaultAsync(token);
            }

            private async Task<User> GetUserByLinkedInEmail(string linkedEmail, CancellationToken token)
            {
                return await _db.UserCollection
                    .AsQueryable()
                    .Where(x => x.Email == linkedEmail)
                    .FirstOrDefaultAsync(token);
            }

            private async Task<LoginItem> GenerateLoginItem(User user, CancellationToken token)
            {
                var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, token);

                return new LoginItem()
                {
                    TokenInfo = tokenResource,
                    User = new TokenUser() {
                        Name = user.Name,
                        Phone = user.Phone,
                        Cpf = user.Cpf,
                        Email = user.Email
                    }
                };
            }

            private async Task<LoginItem> BindUserLinkedIn(User user, LinkedInUserItem linkUser, CancellationToken token)
            {
                user.LinkedInId = linkUser.LinkedInId;

                await _db.UserCollection.ReplaceOneAsync(t =>
                    t.Id == user.Id, user,
                    cancellationToken: token
                );

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, token);

                return new LoginItem()
                {
                    TokenInfo = tokenResource,
                    User = new TokenUser()
                    {
                        Name = user.Name,
                        Phone = user.Phone,
                        Cpf = user.Cpf,
                        Email = user.Email
                    }
                };
            }

            private async Task<LoginItem> CreateNewUserLinkedIn(LinkedInUserItem linkUser, CancellationToken token)
            {
                var user = new Student(
                    linkUser.FirstName,
                    linkUser.FirstName + ' ' + linkUser.LastName,
                    linkUser.Email,
                    linkUser.FirstName.ToUpper(),
                    false
                );

                user.LinkedInId = linkUser.LinkedInId;
                user.FirstAccess = true;
                user.EmailVerified = true;

                var randomPassword = Guid.NewGuid().ToString("d").Substring(1, 6);
                var result = await _userManager.CreateAsync(user, randomPassword);

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, token);

                return new LoginItem()
                {
                    TokenInfo = tokenResource,
                    User = new TokenUser()
                    {
                        Name = user.Name,
                        Phone = user.Phone,
                        Cpf = user.Cpf,
                        Email = user.Email
                    }
                };
            }
        }
    }
}