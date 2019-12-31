using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.QuestionsDraft.Commands;
using Domain.Aggregates.QuestionsDraft.Queries;
using Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class QuestionDraftController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public QuestionDraftController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost()]
        public async Task<IActionResult> ManageQuestionDraft([FromBody] ManageQuestionDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllQuestionsDraft(GetAllQuestionsDraft.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedQuestionsDraft([FromBody]GetPagedQuestionsDraft.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete()]
        public async Task<IActionResult> RemoveQuestionDraft(RemoveQuestionDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("importQdb")]
        public async Task<IActionResult> ImportQdb([FromBody] ImportDraftQdb.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
