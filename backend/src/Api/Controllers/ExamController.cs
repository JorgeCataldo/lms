using System.IO;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.UserProgressHistory.Commands;
using Domain.Aggregates.UserProgressHistory.Queries;
using Domain.Aggregates.Users;
using Infrastructure.ExcelServices.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ExamController: BaseController
    {
        private readonly UserManager<User> _userManager;

        public ExamController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost("startexam")]
        public async Task<IActionResult> StartExam([FromBody]StartExamCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("answer")]
        public async Task<IActionResult> AnswerQuestion([FromBody]AnswerQuestionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("finishExamValuation")]
        public async Task<IActionResult> FinishExamValuation([FromBody]AnswerQuestionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getAnswers")]
        public async Task<IActionResult> GetAnswers([FromBody]GetAnswersQuery.Contract request)
        {
            request.RequestUserId = this.GetUserId();
            
            var userRole = this.GetUserRole();
            if(userRole != "Admin")
                return this.Unauthorized();
                
            var result = await this.Send(request);
            if(result.IsFailure)
                return RestResult.CreateHttpResponse(result);

            var bytes = result.Data.ToExcel("answers");
            return File(new MemoryStream(bytes), "application/octet-stream", "answers.xlsx");
        }
    }
}
