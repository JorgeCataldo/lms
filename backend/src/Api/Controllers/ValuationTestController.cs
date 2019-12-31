using Api.Extensions;
using Domain.Aggregates.ValuationTests.Commands;
using Domain.Aggregates.ValuationTests.Queries;
using Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Tg4.Infrastructure.Web.Message;
using Api.Base;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ValuationTestController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public ValuationTestController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet()]
        public async Task<IActionResult> GetValuationTestsQuery(GetAllValuationTests.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetValuationTestByIdQuery(GetValuationTestByIdQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manage")]
        public async Task<IActionResult> ManageValuationTestCommand([FromBody]ManageValuationTestCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteValuationTest(DeleteValuationTestCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("responses")]
        public async Task<IActionResult> GetValuationTestResponsesQuery([FromBody]GetValuationTestResponses.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("responses/byId")]
        public async Task<IActionResult> GetValuationTestResponseById(GetValuationTestResponseById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("responses/save")]
        public async Task<IActionResult> SaveValuationTestResponseCommand([FromBody]SaveValuationTestResponseCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("responses/all")]
        public async Task<IActionResult> GetAllValuationTestResponses(GetAllValuationTestResponses.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("responses/grade")]
        public async Task<IActionResult> SaveValuationTestAnswersGrades([FromBody]SaveValuationTestAnswersGrades.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("module")]
        public async Task<IActionResult> GetModuleValuationTests(GetModuleValuationTests.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpGet("track")]
        public async Task<IActionResult> GetTrackValuationTests(GetTrackValuationTests.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}