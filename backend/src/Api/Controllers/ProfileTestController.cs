using Api.Base;
using Api.Extensions;
using Domain.Aggregates.ProfileTests.Commands;
using Domain.Aggregates.ProfileTests.Queries;
using Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ProfileTestController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public ProfileTestController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet()]
        public async Task<IActionResult> GetProfileTestsQuery(GetProfileTestsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetProfileTestByIdQuery(GetProfileTestByIdQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manage")]
        public async Task<IActionResult> ManageProfileTestCommand([FromBody]ManageProfileTestCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteProfileTest(DeleteProfileTestCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("responses")]
        public async Task<IActionResult> GetProfileTestResponsesQuery([FromBody]GetProfileTestResponses.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("responses/byId")]
        public async Task<IActionResult> GetProfileTestResponseById(GetProfileTestResponseById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("response/save")]
        public async Task<IActionResult> SaveProfileTestResponseCommand([FromBody]SaveProfileTestResponseCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("responses/all")]
        public async Task<IActionResult> GetAllProfileTestResponses(GetAllProfileTestResponses.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> SuggestProductsCommand([FromBody]SuggestProductsCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedProductsQuery(GetSuggestedProductsQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("suggest/test")]
        public async Task<IActionResult> SuggestProfileTestCommand([FromBody]SuggestProfileTestCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("responses/grade")]
        public async Task<IActionResult> SaveProfileTestAnswersGrades([FromBody]SaveProfileTestAnswersGrades.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
