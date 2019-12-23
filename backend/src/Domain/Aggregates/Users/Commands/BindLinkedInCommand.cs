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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class BindLinkedInCommand
    {
        public class Contract: CommandContract<Result>
        {
            [Required]
            public string Code { get; set; }
            [Required]
            public string UserId { get; set; }
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

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IConfiguration _configuration;
            private readonly IDbContext _db;

            public Handler(IConfiguration configuration, 
                IDbContext db)
            {
                _configuration = configuration;
                _db = db;
            }
            
            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Code)) 
                    return Result.Fail("Codigo inválido");

                if (string.IsNullOrEmpty(request.UserId))
                    return Result.Fail("Usuário inválido");

                var userId = ObjectId.Parse(request.UserId);

                var response = await GetAccessToken(request.Code);
                string content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return Result.Fail("Ocorreu um erro ao se comunicar com o LinkedIn");
                
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

                var linkedUser = await GetUserByLinkedInId(linkedInUser.LinkedInId, cancellationToken);
                if (linkedUser != null)
                    return Result.Fail("Esta conta do LinkedIn já esta associada a um usuário");

                var user = await GetUserByUserId(userId, cancellationToken);
                if (user == null)
                    return Result.Fail("Usuário não existe");

                return Result.Ok(await BindUserLinkedIn(user, linkedInUser, cancellationToken));
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

            private async Task<User> GetUserByUserId(ObjectId userId, CancellationToken token)
            {
                return await _db.UserCollection
                    .AsQueryable()
                    .Where(x => x.Id == userId)
                    .FirstOrDefaultAsync(token);
            }

            private async Task<User> BindUserLinkedIn(User user, LinkedInUserItem linkUser, CancellationToken token)
            {
                user.LinkedInId = linkUser.LinkedInId;

                await _db.UserCollection.ReplaceOneAsync(t =>
                    t.Id == user.Id, user,
                    cancellationToken: token
                );

                return user;
            }
        }
    }
}