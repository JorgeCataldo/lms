using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.EventsDrafts.Commands;
using Domain.Aggregates.EventsDrafts.Queries;
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
    public class EventDraftController : BaseController
    {
        private UserManager<User> _userManager;

        public EventDraftController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedEventsAndDrafts([FromBody]GetPagedEventsAndDraftsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetEventDraftById(GetEventDraftById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost()]
        public async Task<IActionResult> AddEventDraft([FromBody] AddEventDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateEventDraft([FromBody] UpdateEventDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSupportMaterials")]
        public async Task<IActionResult> ManageSupportMaterials([FromBody] ManageDraftSupportMaterials.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageRequirements")]
        public async Task<IActionResult> ManageRequirements([FromBody] ManageDraftRequirements.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSchedule")]
        public async Task<IActionResult> ManageSchedule([FromBody] ManageDraftSchedule.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("deleteEvent")]
        public async Task<IActionResult> DeleteModule([FromBody] DeleteEventDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("publishDraft")]
        public async Task<IActionResult> PublishDraft(PublishEventDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
