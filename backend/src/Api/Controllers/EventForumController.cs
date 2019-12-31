using Api.Base;
using Api.Extensions;
using Domain.Aggregates.EventForums.Commands;
using Domain.Aggregates.EventForums.Queries;
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
    public class EventForumController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public EventForumController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost("eventForumByEventSchedule")]
        public async Task<IActionResult> GetEventForumByModule([FromBody] GetEventForumByEventScheduleQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("saveEventForumQuestion")]
        public async Task<IActionResult> SaveEventForumQuestion([FromBody] SaveEventForumQuestionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("previewEventForumQuestions")]
        public async Task<IActionResult> GetPreviewForumQuestion([FromBody] GetEventForumPreviewByEventScheduleQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPut("manageEventForumQuestionLike")]
        public async Task<IActionResult> ManageQuestionLike([FromBody] ManageEventForumQuestionLikeCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("eventForumQuestion")]
        public async Task<IActionResult> GetQuestionById(GetEventForumQuestionByIdQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("eventForumQuestion/answer")]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveEventForumAnswerCommand.Contract request)
        {
            request.User = await _userManager.GetUserAsync(User);
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("eventForumQuestion/answer/manageLike")]
        public async Task<IActionResult> ManageAnswerLike([FromBody] ManageEventForumAnswerLikeCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("eventForumQuestion/remove")]
        public async Task<IActionResult> RemoveQuestion([FromBody] RemoveEventForumQuestionCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("eventforumQuestion/remove/answer")]
        public async Task<IActionResult> RemoveAnswer([FromBody] RemoveEventForumAnswerCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("userPreviewEventForumQuestions")]
        public async Task<IActionResult> GetUserPreviewForumQuestion([FromBody] GetUserEventForumPreviewByEventScheduleQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
