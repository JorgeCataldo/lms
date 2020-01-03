using System.Threading.Tasks;
using Domain.Aggregates.Tracks.Commands;
using Domain.Aggregates.Tracks.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;
using Api.Extensions;
using Microsoft.AspNetCore.Identity;
using Domain.Aggregates.Users;
using System;
using System.IO;
using Api.Base;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TrackController: BaseController
    {
        private readonly UserManager<User> _userManager;

        public TrackController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost()]
        public async Task<IActionResult> AddOrModifyQuestion([FromBody] AddOrModifyTrackCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpGet()]
        public async Task<IActionResult> GetById(GetTrackByIdQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered")]
        public async Task<IActionResult> GetAllPagedFiltered([FromBody]GetTracksPagedQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPost("filtered/manager")]
        public async Task<IActionResult> GetManagerPagedFiltered([FromBody]GetManagerTracksPagedQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered/effortPerformance")]
        public async Task<IActionResult> GetEffortPerformancePagedFiltered([FromBody]GetTracksEffortPerformancesPagedQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("deleteTrack")]
        public async Task<IActionResult> DeleteTrack([FromBody] DeleteTrackCommand.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete()]
        public async Task<IActionResult> RemoveQuestion(RemoveTrackCommand.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetTrackOverview(GetTrackOverviewQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/eventInfo")]
        public async Task<IActionResult> GetTrackOverviewEventInfo(GetTrackOverviewEventInfoQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/student/current")]
        public async Task<IActionResult> GetTrackCurrentStudentOverviewQuery(GetTrackCurrentStudentOverviewQuery.Contract request)
        {
            request.StudentId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/student")]
        public async Task<IActionResult> GetTrackStudentOverviewQuery(GetTrackStudentOverviewQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/module")]
        public async Task<IActionResult> GetTrackModuleOverviewQuery(GetTrackModuleOverviewQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("overview/video/markViewed")]
        public async Task<IActionResult> MarkMandatoryVideoViewed([FromBody] MarkMandatoryVideoViewedCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageCalendarEvents")]
        public async Task<IActionResult> ManageTrackCalendarEvents([FromBody] ManageTrackCalendarEventsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageEcommerceProducts")]
        public async Task<IActionResult> ManageEcommerceProducts([FromBody] ManageEcommerceProducts.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/grades")]
        public async Task<IActionResult> ExportTrackGrades(GetTrackOverviewGradesQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview/students")]
        public async Task<IActionResult> ExportTrackStudents(GetTrackOverviewStudentsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("addCalendarEventsFromFile")]
        public async Task<IActionResult> AddCalendarEventsFromFile([FromBody] AddCalendarEventsFromFileCommand.Contract request)
        {
            
            var role = this.GetUserRole();
            if (role != "Admin")
                return this.Unauthorized();

            request.FileContent = request.FileContent.Substring(request.FileContent.IndexOf(",", StringComparison.Ordinal) + 1);
            Byte[] bytes = Convert.FromBase64String(request.FileContent);
            using (Stream stream = new MemoryStream(bytes))
            {
                StreamReader reader = new StreamReader(stream);
                request.FileContent = reader.ReadToEnd();
            }

            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getAllContent")]
        public async Task<IActionResult> GetAllContent(GetAllContentQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getAllTracks")]
        public async Task<IActionResult> GetAllTracks(GetAllTrackQuery.Contract request)
        {

            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getNotRecommendedTracks")]
        public async Task<IActionResult> GetNotRecommendedTracks([FromBody]GetNotRecommendedTrackQuery.Contract request)
        {

            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

    }
}
