using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.Users;
using Domain.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Domain.IdentityStores.Settings
{
	public class TokenGenerator : ITokenGenerator
	{
		private readonly UserManager<User> _userManager;
		private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IDbContext _db;

        public TokenGenerator(
			UserManager<User> userManager,
			IOptions<JwtOptions> jwtOptions,
            IDbContext db)
		{
			_userManager = userManager;
			_jwtOptions = jwtOptions;
            _db = db;
		}

		public async Task<TokenInfo> GenerateUserToken(User user)
		{
			var now = DateTime.UtcNow;

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64)
			};

			var roleClaims = await _userManager.GetRolesAsync(user);
			claims.AddRange(roleClaims.Select(x => new Claim(ClaimTypes.Role, x)));
            claims.Add(new Claim(ClaimTypes.Role, user.GetUserRole(user)));

			//var userClaims = await _userManager.GetClaimsAsync(user);
			//claims.AddRange(userClaims.Select(x => new Claim(x.Type, x.Value)));

			var jwt = new JwtSecurityToken(
				_jwtOptions.Value.JwtIssuer,
				_jwtOptions.Value.JwtAudience,
				claims,
				now,
				now.Add(TimeSpan.FromMinutes(_jwtOptions.Value.JwtExpirationMinutes)),
				new SigningCredentials(
					new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Value.JwtSecret)),
					SecurityAlgorithms.HmacSha256));

			var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            var role = user.GetUserRole(user);

            return new TokenInfo
            {
                AccessToken = token,
                ExpiresIn = (int)TimeSpan.FromMinutes(_jwtOptions.Value.JwtExpirationMinutes).TotalSeconds,
                Username = user.UserName,
                RefreshToken = Guid.NewGuid(),
                UserId = user.Id.ToString(),
                Email = user.Email,
                Role = role,
                CompletedRegister = true,
                ImageUrl = user.ImageUrl,
                Dependents = await CheckUserDependent(user),
                Name = user.Name,
                FirstAccess = user.FirstAccess,
                EmailVerified = user.EmailVerified,
                AdminAccesses = await GetUserAccesses(role, user.Id)
            };
		}

        private async Task<bool> CheckUserDependent(User user)
        {
            var filters = FilterDefinition<User>.Empty;
            var builder = Builders<User>.Filter;
            filters = filters & builder.Regex("lineManager", new BsonRegularExpression("/" + user.Name + "/is"));

            CancellationTokenSource sourceToken = new CancellationTokenSource();
            CancellationToken token = sourceToken.Token;

            var collection = _db.Database.GetCollection<User>("Users");
            var qry = await collection.FindAsync(filters,
                options: new FindOptions<User>(),
                cancellationToken: token
            );
            var usersList = await qry.ToListAsync(token);

            if(usersList != null && usersList.Count > 0)
            {
                return true;
            }

            return false;
        }

        private async Task<List<string>> GetUserAccesses(string role, ObjectId userId)
        {
            var accesses = new List<string>();

            if (role == "Student")
            {
                var queryModules = await _db.Database
                .GetCollection<Module>("Modules")
                .FindAsync(x => x.InstructorId == userId || x.ExtraInstructorIds.Contains(userId) || x.TutorsIds.Contains(userId));

                var teachesModules = await queryModules.AnyAsync();
                if (teachesModules)
                    accesses.Add("Modules");

                var queryEvents = await _db.Database
                    .GetCollection<Event>("Events")
                    .FindAsync(x => x.InstructorId == userId || x.TutorsIds.Contains(userId));

                var teachesEvents = await queryEvents.AnyAsync();
                if (teachesEvents)
                    accesses.Add("Events");

                var queryGestor = await _db.Database
                    .GetCollection<Responsible>("Responsibles")
                    .FindAsync(x => x.ResponsibleUserId == userId);

                var isGestor = await queryGestor.AnyAsync();
                if (isGestor)
                {
                    accesses.Add("Gestor");
                }
                else
                {
                    var querySubordinate = await _db.Database
                       .GetCollection<Responsible>("Responsibles")
                       .FindAsync(x => x.SubordinatesUsersIds.Contains(userId));

                    var subordinate = await querySubordinate.FirstOrDefaultAsync();
                    if (subordinate != null)
                    {
                        var responsibleUser = await (await _db.Database
                            .GetCollection<User>("Users")
                            .FindAsync(x => x.Id == subordinate.ResponsibleUserId)).FirstOrDefaultAsync();

                        if (responsibleUser != null && responsibleUser.GetUserRole(responsibleUser) == "BusinessManager")
                        {
                            accesses.Add("BusinessManagerStudent");
                        }
                    }
                }
            }
            else if (role == "BusinessManager")
            {
                var queryGestor = await _db.Database
                    .GetCollection<Responsible>("Responsibles")
                    .FindAsync(x => x.ResponsibleUserId == userId);

                var isGestor = await queryGestor.AnyAsync();
                if (isGestor)
                    accesses.Add("Gestor");
            }

            return accesses;
        }

    }
}