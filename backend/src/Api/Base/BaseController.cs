using Domain.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Base
{
    public abstract class BaseController : Controller
    {
        private readonly IMediator _mediator;

        protected BaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected Task<TResponse> Send<TResponse>(
            CommandContract<TResponse> request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            request.AuthenticationData = this.GetAuthenticationData();
            return this._mediator.Send(request, cancellationToken);
        }

        private AuthenticationData GetAuthenticationData()
        {
            if (!this.User.Identity.IsAuthenticated)
                return null;

            var userId = this.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var impersonateId = this.User.Claims.FirstOrDefault(x => x.Type == "UserIdToImpersonate")?.Value;
            var impersonateRole = this.User.Claims.FirstOrDefault(x => x.Type == "UserRoleToImpersonate")?.Value;

            var auth = new AuthenticationData()
            {
                UserId = ObjectId.Parse(impersonateId ?? userId),
                LoggedUserId = ObjectId.Parse(userId),
                ImpersonatedUserId = impersonateId != null ? ObjectId.Parse(impersonateId) : (ObjectId?)null,
                ImpersonatedUserRole = impersonateRole,
            };

            return auth;
        }
    }
}
