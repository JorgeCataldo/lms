using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.ColorPalettes;
using Domain.Aggregates.Responsibles;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users.Commands
{
    public class Login
    {
        public class Contract : CommandContract<Result<LoginItem>>
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
        }

        public class LoginItem
        {
            public TokenInfo TokenInfo { get; set; }
            public TokenUser User { get; set; }
        }

        public class Handler : BaseAccountCommand, IRequestHandler<Contract, Result<LoginItem>>
        {
            private readonly UserManager<User> _userManager;
            private readonly ITokenGenerator _tokenGenerator;
            private readonly IDbContext _db;

            public Handler(
                IDbContext context,
                UserManager<User> userManager,
                ITokenGenerator tokenGenerator): base(context)
            {
                _userManager = userManager;
                _tokenGenerator = tokenGenerator;
                _db = context;
            }
            
            public async Task<Result<LoginItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByNameAsync(request.UserName);
            
                if (user == null || user.IsBlocked)
                    return Result.Fail<LoginItem>("Usuário ou Senha inválido");

                var result = await _userManager.CheckPasswordAsync(user, request.Password);

                if (!result)
                    return Result.Fail<LoginItem>("Usuário ou Senha inválido");

                var tokenResource = await _tokenGenerator.GenerateUserToken(user);
                var tokenUser = new TokenUser() { Name = user.Name, Phone = user.Phone, Cpf = user.Cpf, Email = user.Email };
                tokenUser.ColorBaseValues = CheckUserColorPalette(tokenResource.UserId, tokenResource.Role);
                tokenUser.LogoUrl = CheckUserLogo(tokenResource.UserId, tokenResource.Role);

                await SaveRefreshToken(user.Id, tokenResource.RefreshToken, cancellationToken);
                
                return Result.Ok(new LoginItem() { TokenInfo = tokenResource, User = tokenUser });
            }

            private List<ColorBaseValue> CheckUserColorPalette(string Id, string userType)
            {
                if (userType != "Student" && userType != "BusinessManager")
                    return new List<ColorBaseValue>();

                var userId = ObjectId.Parse(Id);
                if (userType == "BusinessManager")
                {
                    var colorPallete = _db.ColorPaletteCollection
                        .AsQueryable()
                        .Where(x => x.UserId == userId)
                        .FirstOrDefault();

                    if (colorPallete == null)
                        return new List<ColorBaseValue>();

                    return colorPallete.ColorBaseValues;
                }
                else
                {
                    var responsible = _db.ResponsibleCollection
                       .AsQueryable()
                       .Where(x => x.SubordinatesUsersIds.Contains(userId))
                       .FirstOrDefault();

                    if (responsible == null)
                        return new List<ColorBaseValue>();

                    var colorPallete = _db.ColorPaletteCollection
                       .AsQueryable()
                       .Where(x => x.UserId == responsible.ResponsibleUserId)
                       .FirstOrDefault();

                    if (colorPallete == null)
                        return new List<ColorBaseValue>();

                    return colorPallete.ColorBaseValues;
                }
            }

            private string CheckUserLogo(string Id, string userType)
            {
                if (userType != "Student" && userType != "BusinessManager")
                    return null;

                var userId = ObjectId.Parse(Id);
                if (userType == "BusinessManager")
                {
                    var logo = _db.RecruitingCompanyCollection
                        .AsQueryable()
                        .Where(x => x.CreatedBy == userId)
                        .Select(x => x.LogoUrl)
                        .FirstOrDefault();

                    return logo;
                }
                else
                {
                    var responsible = _db.ResponsibleCollection
                       .AsQueryable()
                       .Where(x => x.SubordinatesUsersIds.Contains(userId))
                       .FirstOrDefault();

                    if (responsible == null)
                        return null;

                    var logo = _db.RecruitingCompanyCollection
                        .AsQueryable()
                        .Where(x => x.CreatedBy == responsible.ResponsibleUserId)
                        .Select(x => x.LogoUrl)
                        .FirstOrDefault();

                    return logo;
                }
            }
        }
    }
}