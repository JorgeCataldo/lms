using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Modules.Commands;
using Domain.Aggregates.Questions.Commands;
using Domain.Aggregates.Questions.Queries;
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
    public class QuestionController: BaseController
    {
        private readonly UserManager<User> _userManager;

        public QuestionController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }
        
        [HttpPost()]
        public async Task<IActionResult> AddOrModifyQuestion([FromBody] AddOrModifyQuestionCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpGet()]
        public async Task<IActionResult> GetAllPaged(GetQuestionsPagedQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered")]
        public async Task<IActionResult> GetAllPagedFiltered([FromBody]GetQuestionsPagedQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("validateModuleQuestions")]
        public async Task<IActionResult> ValidateModuleQuestions([FromBody]ValidateModuleQuestionsQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("ImportQdb")]
        public async Task<IActionResult> ImportQdb([FromBody] ImportDraftQdb.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getAllQuestions")]
        public async Task<IActionResult> ExportQdb([FromBody] GetAllQuestionsQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpDelete()]
        public async Task<IActionResult> RemoveQuestion(RemoveQuestionCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
