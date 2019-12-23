using System;
using System.Text;
using Domain.Aggregates.Users;
using Domain.IdentityStores;
using Domain.IdentityStores.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions
{

    public static class AuthExtension
    {
        private static string _secretKey;
        private static string _issuer;
        private static string _audience;

        private static SymmetricSecurityKey _signingKey;

        public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("JwtAuthentication");
            var cfg = new JwtOptions();
            section.Bind(cfg);

            _secretKey = cfg.JwtSecret;
            _issuer = cfg.JwtIssuer;
            _audience = cfg.JwtAudience;

            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

            services.AddIdentity<User, IdentityRole>(config =>
                {
                    config.Password.RequireDigit = false;
                    config.Password.RequiredLength = 5;
                    config.Password.RequireLowercase = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;

                    config.User.RequireUniqueEmail = false;

                    config.SignIn.RequireConfirmedEmail = true;

                })
                .AddUserStore<UserStore>()
                .AddDefaultTokenProviders();

            var tokenValidationParameters = new TokenValidationParameters
            {
                //The signing key must match !
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                //Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = _issuer,

                //validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = _audience,

                //validate the token expiry
                ValidateLifetime = true,

                // If you  want to allow a certain amout of clock drift
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });

            configuration.GetSection("JwtAuthentication");

            services.Configure<JwtOptions>(configuration.GetSection("JwtAuthentication"));
        }
    }  
}