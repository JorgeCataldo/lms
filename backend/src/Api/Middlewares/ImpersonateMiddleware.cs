using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Middlewares
{
    public static class ImpersonateMiddlewareExtensions
    {
        public static IApplicationBuilder UseImpersonate(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImpersonateMiddleware>();
        }
    }

    public class ImpersonateMiddleware
    {
        private readonly RequestDelegate _next;

        public ImpersonateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IMemoryCache memoryCache)
        {
            if (!string.IsNullOrEmpty(context.Request.Headers["UserIdToImpersonate"]))
            {
                if (context.User.IsInRole("Admin") || context.User.IsInRole("Impersonate"))
                {
                    var userIdToImpersonate =
                        context.Request.Headers["UserIdToImpersonate"].ToString();

                    var userRoleToImpersonate =
                        context.Request.Headers["UserRoleToImpersonate"].ToString();

                    var userId = context.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                    var hasAccess = context.User.IsInRole("Admin");

                    if (hasAccess)
                    {
                        context.User.Identities.First().AddClaim(new Claim("UserIdToImpersonate", userIdToImpersonate));
                        context.User.Identities.First().AddClaim(new Claim("UserRoleToImpersonate", userRoleToImpersonate));
                    }
                }
            }

            if (!string.IsNullOrEmpty(context.Request.Headers["UserRoleToSeeHow"]))
            {
                if (context.User.IsInRole("Admin"))
                {
                    var userRoleToSeeHow =
                        context.Request.Headers["UserRoleToSeeHow"].ToString();

                    var hasAccess = context.User.IsInRole("Admin");

                    if (hasAccess)
                        context.User.Identities.First().AddClaim(new Claim("UserRoleToSeeHow", userRoleToSeeHow));
                }
            }
            await _next(context);
        }
    }
}
