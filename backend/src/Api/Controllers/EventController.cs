using System;
using System.IO;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Events.Commands;
using Domain.Aggregates.Events.Queries;
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
    public class EventController : BaseController
    {
        private UserManager<User> _userManager;

        public EventController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }
        
        [HttpPost()]
        public async Task<IActionResult> AddOrModifyEvent([FromBody] AddOrModifyEventCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPost("filtered")]
        public async Task<IActionResult> GetAllPagedFiltered([FromBody]GetPagedEventsQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("homeEvents")]
        public async Task<IActionResult> GetAllHomePagedFiltered(GetPagedHomeEventsQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet()]
        public async Task<IActionResult> GetById(GetEventByIdQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        [HttpPost("manageSupportMaterials")]
        public async Task<IActionResult> ManageSupportMaterials([FromBody] ManageSupportMaterialsCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageRequirements")]
        public async Task<IActionResult> ManageRequirements([FromBody] ManageRequirementsCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSchedule")]
        public async Task<IActionResult> ManageSchedule([FromBody] ManageScheduleCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpGet("getEventApplicationByUserQuery")]
        public async Task<IActionResult> GetEventApplicationByUserQuery(GetEventApplicationByUserQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getEventApplicationByEventId")]
        public async Task<IActionResult> GetEventApplicationByEventIdQuery([FromBody] GetEventApplicationByEventIdQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getEventSchedulesByEventId")]
        public async Task<IActionResult> GetEventSchedulesByEventIdQuery([FromBody] GetEventSchedulesByEventIdQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeEventPublishedStatus")]
        public async Task<IActionResult> ChangeUserBlockedStatus([FromBody] ChangeEventSchedulePublishedStatusCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("applyToEvent")]
        public async Task<IActionResult> ApplyToEvent([FromBody] ApplyToEventCommand.Contract request)
        {
            
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeEventUserApplicationStatus")]
        public async Task<IActionResult> ChangeEventUserApplicationStatus([FromBody] ChangeEventUserApplicationStatusCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.ManagerUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeEventUserGrade")]
        public async Task<IActionResult> ChangeEventUserGrade([FromBody] ChangeEventUserGradeCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeEventUserForumGrade")]
        public async Task<IActionResult> ChangeEventUserForumGrade([FromBody] ChangeEventUserForumGradeCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("deleteEvent")]
        public async Task<IActionResult> DeleteModule([FromBody] DeleteEventCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getEventReactions")]
        public async Task<IActionResult> GetEventReactions(GetEventReactionResultsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPost("addEventReaction")]
        public async Task<IActionResult> AddEventReaction([FromBody] AddEventReactionCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageEventReaction")]
        public async Task<IActionResult> ManageEventReaction([FromBody] ManageEventReactionSuggestionCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageUserPresence")]
        public async Task<IActionResult> ManageUserPresence([FromBody] ManageUserPresenceCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("finishEvent")]
        public async Task<IActionResult> FinishEvent([FromBody] FinishEventCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("sendEventReactionEvaluation")]
        public async Task<IActionResult> ValuateEvent([FromBody] SendEventReactionEvaluationEmailCommand.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("GetEventPrepAnswersByIdQuery")]
        public async Task<IActionResult> GetEventPrepAnswersByIdQuery([FromBody] GetEventPrepAnswersByIdQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("pastEvents")]
        public async Task<IActionResult> GetPastEvents(GetPastEventsQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpGet("GetAllEventsByUser")]
        public async Task<IActionResult> GetAllEvents(GetAllEventsByUserQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeUserEventApplicationSchedule")]
        public async Task<IActionResult> ChangeUserEventApplicationSchedule([FromBody] ChangeUserEventApplicationSchedule.Contract request)
        {
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("AddEventUsersGradeBaseValues")]
        public async Task<IActionResult> AddEventUsersGradeBaseValues([FromBody] ImportEventUsersGradeBaseValuesCommand.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin")
                return this.Unauthorized();

            //request.FileContent = request.FileContent.Substring(request.FileContent.IndexOf(",", StringComparison.Ordinal) + 1);
            //Byte[] bytes = Convert.FromBase64String(request.FileContent);
            //using (Stream stream = new MemoryStream(bytes))
            //{
            //    StreamReader reader = new StreamReader(stream);
            //    request.FileContent = reader.ReadToEnd();
            //}

            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("GetEventStudentList")]
        public async Task<IActionResult> GetEventStudentList(ExportEventUsersGradesTemplates.Contract request)
        {
            var role = this.GetUserRole();
            if (role != "Admin" && role != "Author")
                return this.Unauthorized();
            
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
