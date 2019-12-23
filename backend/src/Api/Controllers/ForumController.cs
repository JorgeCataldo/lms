using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Forums.Commands;
using Domain.Aggregates.Forums.Queries;
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
    public class ForumController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public ForumController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost("forumByModule")]
        public async Task<IActionResult> GetForumByModule([FromBody] GetForumByModuleQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("saveQuestion")]
        public async Task<IActionResult> SaveForumQuestion([FromBody] SaveForumQuestionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("previewForumQuestions")]
        public async Task<IActionResult> GetPreviewForumQuestion([FromBody] GetForumPreviewByModuleQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPut("manageQuestionLike")]
        public async Task<IActionResult> ManageQuestionLike([FromBody] ManageQuestionLikeCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("question")]
        public async Task<IActionResult> GetQuestionById(GetQuestionByIdQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("question/answer")]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerCommand.Contract request)
        {
            request.User = await _userManager.GetUserAsync(User);
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("question/answer/manageLike")]
        public async Task<IActionResult> ManageAnswerLike([FromBody] ManageAnswerLikeCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("question/remove")]
        public async Task<IActionResult> RemoveQuestion([FromBody] RemoveForumQuestionCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("question/remove/answer")]
        public async Task<IActionResult> RemoveAnswer([FromBody] RemoveForumAnswerCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
