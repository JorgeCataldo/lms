using System;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Activations.Commands;
using Domain.Aggregates.Activations.Queries;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Commands;
using Domain.Aggregates.VerificationEmail.Commands;
using Domain.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tg4.Infrastructure.Functional;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AccountController: Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator, UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mediator = mediator;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("linkedIn")]
        public async Task<IActionResult> LinkedIn([FromBody] LoginLinkedIn.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("bindLinkedIn")]
        public async Task<IActionResult> BindLinkedIn([FromBody] BindLinkedInCommand.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("FirstAccess")]
        public async Task<IActionResult> FirstAccess([FromBody] FirstAccess.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("sso")]
        public IActionResult Sso()
        {
            var section = _configuration.GetSection("SsoSettings");
            var cfg = new SamlAuthenticationOptions();
            section.Bind(cfg);

            //specify the SAML provider url here, aka "Endpoint"
            var samlEndpoint = cfg.SamlEndpoint;

            var request = new AuthRequest(
                cfg.AppIdURI, //put your app's "unique ID" here
                cfg.RedirectUrl //assertion Consumer Url - the redirect URL where the provider will send authenticated users
            );

            //generate the provider URL
            string url = request.GetRedirectUrl(samlEndpoint);

            //then redirect your user to the above "url" var
            //for example, like this:
            return Redirect(url);
        }

        [HttpPost("auth-success")]
        public async Task<IActionResult> SsoSuccess()
        {
            var request = new LoginSso.Contract() { SamlResponse = Request.Form["SAMLResponse"] };
            var result = await _mediator.Send(request);

            //redirecionar novamente
            return Redirect(result.Data.UrlRedirect);
        }


        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("SendVerificationEmail")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmail.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("VerifyEmailCode")]
        public async Task<IActionResult> VerifyEmail([FromBody] CheckVerificationEmail.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [Authorize]
        [HttpPost("setFirstAccess")]
        public async Task<IActionResult> SetFirstAccess([FromBody] SetFirstAccessStatusCommand.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin")
                return this.Unauthorized();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [Authorize]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUser.Contract request)
        {
            var user = await _userManager.GetUserAsync(User);
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [Authorize]
        [HttpPost("Edit")]
        public async Task<IActionResult> Edit([FromBody] EditUser.Contract request){
            request.UserId = this.GetUserId();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [Authorize]
        [HttpPost("AdminChangePassword")]
        public async Task<IActionResult> AdminChangePassword([FromBody] AdminChangePassword.Contract request)
        {
            

            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }   
}