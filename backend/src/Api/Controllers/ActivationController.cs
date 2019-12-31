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
    [Authorize]
    [Route("api/[controller]")]
    public class ActivationController : BaseController
    {

        public ActivationController(IMediator mediator): base(mediator)
        {
        }

        [HttpGet("GetActivations")]
        public async Task<IActionResult> GetActivations(GetActivationsQuery.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("GetActivationByType")]
        public async Task<IActionResult> GetActivationByType(GetActivationByTypeQuery.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("UpdateActivationStatus")]
        public async Task<IActionResult> UpdateActivationStatus([FromBody] UpdateActivationStatusCommand.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("CreateCustomActivation")]
        public async Task<IActionResult> CreateCustomActivation([FromBody] CreateCustomActivationCommand.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("UpdateCustomActivation")]
        public async Task<IActionResult> UpdateCustomActivation([FromBody] UpdateCustomActivationCommand.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteActivation(DeleteActivationCommand.Contract request)
        {

            var role = this.GetUserRole();
            if (role != "Admin" && role != "Secretary")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
        
}