using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Actions.Queries;
using Domain.Aggregates.Modules.Commands;
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
    public class AnalyticsController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public AnalyticsController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet()]
        public async Task<IActionResult> SaveAction(SaveActionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("removeAction")]
        public async Task<IActionResult> RemoveAction([FromBody]RemoveActionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("listActions")]
        public async Task<IActionResult> ListActions(GetPagedActionsQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
