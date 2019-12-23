using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using Domain.Options;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class RegisterUser
    {
        public class Contract : CommandContract<Result<TokenInfo>>
        {
            [Required]
            public string Name { get; set; }
            public string Cpf { get; set; }
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
            public string Info { get; set; }
            [Required]
            public string Email { get; set; }
            public string Phone { get; set; }
            public string ImageUrl { get; set; }
            public string Role { get; set; }
            public string UserRole { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<TokenInfo>>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, 
                UserManager<User> userManager,
                ITokenGenerator tokenGenerator,
                IConfiguration configuration) : base(db)
            {
                _db = db;
                _userManager = userManager;
                _tokenGenerator = tokenGenerator;
                _configuration = configuration;
            }
            
            public async Task<Result<TokenInfo>> Handle(Contract request, CancellationToken cancellationToken)
            {
                string config = _configuration[$"Permissions:AllowSignUp"];
                string autoGenerateRegistrationId = _configuration[$"Permissions:AutoGenerateRegistrationId"];

                if (config == "True" || request.UserRole == "Admin")
                {
                    var emailResult = Email.Create(request.Email);
                    if (emailResult.IsFailure)
                        return Result.Fail<TokenInfo>(emailResult.Error);
                    if (request.UserRole != "Admin")
                        request.Role = "Student";

                    User user = null;
                    switch (request.Role)
                    {
                        case "Admin":
                            user = new Admin(request.Username, request.Name, request.Email, request.Username.ToUpper(), false);
                            break;
                        case "HumanResources":
                            user = new HumanResources(request.Username, request.Name, request.Email, request.Username.ToUpper(), false);
                            break;
                        case "Author":
                            user = new Author(request.Username, request.Name, request.Email, request.Username.ToUpper(), false);
                            break;
                        case "Secretary":
                            user = new Secretary(request.Username, request.Name, request.Email, request.Username.ToUpper(), false);
                            break;
                        case "Student":
                            user = new Student(request.Username, request.Name, request.Email, request.Username.ToUpper(), false);
                            break;
                        default:
                            return Result.Fail<TokenInfo>("Role nao existe");
                    }
                    user.FirstAccess = false;
                    user.EmailVerified = false;
                    user.Cpf = request.Cpf;
                    user.Info = request.Info;
                    user.Phone = request.Phone;
                    user.ImageUrl = request.ImageUrl;

                    if (autoGenerateRegistrationId == "True")
                        user.RegistrationId = await GetNextRegistrationId(cancellationToken);

                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        var error = result.Errors.FirstOrDefault();
                        return error != null ? Result.Fail<TokenInfo>(error.Description) : Result.Fail<TokenInfo>("Erro interno ao criar usuário");
                    }

                    var tokenResource = await _tokenGenerator.GenerateUserToken(user);

                    await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);

                    return Result.Ok(tokenResource);
                }
                return Result.Fail<TokenInfo>("Acesso Negado");
            }

            private async Task<string> GetNextRegistrationId(CancellationToken token)
            {
                var userCount = await _db.Database
                    .GetCollection<User>("Users")
                    .EstimatedDocumentCountAsync(cancellationToken: token);

                string prefix = DateTimeOffset.Now.Year.ToString();
                return prefix + (userCount + 1);
            }
        }
    }
}