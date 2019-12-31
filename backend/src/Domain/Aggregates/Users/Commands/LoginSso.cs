using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Domain.Auth;
using Domain.Data;
using Sentry;
using Domain.IdentityStores.Settings;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Functional;
using Domain.Base;

namespace Domain.Aggregates.Users.Commands
{

    public class LoginSso
    {
        public class Contract : CommandContract<Result<LoginItem>>
        {
            [Required]
            public string SamlResponse { get; set; }
        }

        public class LoginItem
        {
            public TokenInfo TokenInfo { get; set; }
            public string UrlRedirect { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<LoginItem>>
        {
            private readonly UserManager<User> _userManager;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly SamlAuthenticationOptions _samlOptions;
            private readonly IOptions<DomainOptions> _domainOptions;

            public Handler(
                IDbContext context,
                UserManager<User> userManager,
                ITokenGenerator tokenGenerator,
                IConfiguration configuration,
                IOptions<DomainOptions> domainOptions) : base(context)
            {
                var section = configuration.GetSection("SsoSettings");
                var cfg = new SamlAuthenticationOptions();
                section.Bind(cfg);
                _samlOptions = cfg;
                _userManager = userManager;
                _tokenGenerator = tokenGenerator;
                _domainOptions = domainOptions;
            }

            public async Task<Result<LoginItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    //specify the certificate that your SAML provider has given to you
                    Response samlResponse = new Response(_samlOptions.SamlCertificate);
                    samlResponse.LoadXmlFromBase64(request
                        .SamlResponse); //SAML providers usually POST the data into this var
                    Console.WriteLine(samlResponse.Xml);
                    if (!samlResponse.IsValid())
                    {
                        var msg = "Erro na resposta do serviço de autenticação";
                        var exc = new Exception($"Erro na resposta do serviço de autenticação: {samlResponse.IsValidText()}", new Exception(samlResponse.Xml));
                        SentrySdk.CaptureException(exc);
                        return BuildErrorResult(msg);
                    }

                    //WOOHOO!!! user is logged in

                    //Some more optional stuff for you
                    //lets extract username/firstname etc
                    string username, email, firstname, lastname;
                    try
                    {
                        username = samlResponse.GetNameID();
                        email = samlResponse.GetEmail();
                        firstname = samlResponse.GetFirstName();
                        lastname = samlResponse.GetLastName();
                    }
                    catch (Exception ex)
                    {
                        var msg = "Erro ao obter os dados de autenticação";
                        var exc = new Exception($"Erro ao obter os dados de autenticação", new Exception(samlResponse.Xml));
                        SentrySdk.CaptureException(exc);
                        return BuildErrorResult(msg);
                    }

                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        var msg = $"Usuário não encontrado para o email: {email}";
                        var exc = new Exception($"Usuário não encontrado para o email", new Exception(samlResponse.Xml));
                        SentrySdk.CaptureException(exc);
                        return BuildErrorResult(msg);
                    }

                    if (user.IsBlocked)
                    {
                        var msg = "Usuário bloqueado";
                        return BuildErrorResult(msg);
                    }

                    var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                    await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);

                    var jobj = JObject.Parse(JsonConvert.SerializeObject(tokenResource));
                    var queryString = jobj.Properties()
                        .Select(p => p.Name + "=" + WebUtility.UrlEncode(p.Value.ToString()))
                        .Aggregate((x, y) => x + "&" + y);

                    return Result.Ok(new LoginItem()
                    {
                        UrlRedirect = $"{_domainOptions.Value.SiteUrl}/login?ssosuccess=true&{queryString}"
                    });
                }
                catch (Exception err)
                {
                    return BuildErrorResult(err.Message);
                }
            }

            private Result<LoginItem> BuildErrorResult(string msg)
            {
                msg = WebUtility.UrlEncode(msg);
                return Result.Ok(new LoginItem()
                {
                    UrlRedirect = $"{_domainOptions.Value.SiteUrl}/login?ssoerror={msg}"
                });
            }
        }
    }
}
