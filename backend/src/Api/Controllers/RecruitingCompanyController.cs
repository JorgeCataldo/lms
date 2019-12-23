using Api.Base;
using Api.Extensions;
using Domain.Aggregates.JobPosition.Commands;
using Domain.Aggregates.JobPosition.Queries;
using Domain.Aggregates.Notifications.Queries;
using Domain.Aggregates.RecruitingCompany.Commands;
using Domain.Aggregates.RecruitingCompany.Queries;
using Domain.Aggregates.RecruitmentFavorite.Commands;
using Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Threading.Tasks;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RecruitingCompanyController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public RecruitingCompanyController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetRecruitingCompany(GetRecruitingCompany.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manage")]
        public async Task<IActionResult> ManageRecruitingCompany([FromBody]ManageRecruitingCompany.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("availableCandidates")]
        public async Task<IActionResult> GetAvailableCandidates(GetAvailableCandidates.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("favorite")]
        public async Task<IActionResult> AddRecruitmentFavorite([FromBody]AddRecruitmentFavorite.Contract request)
        {
            
            request.RecruiterRole = this.GetUserRole();
            request.RecruiterId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete("favorite")]
        public async Task<IActionResult> RemoveRecruitmentFavorite(RemoveRecruitmentFavorite.Contract request)
        {
            
            request.RecruiterRole = this.GetUserRole();
            request.RecruiterId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("talents/list")]
        public async Task<IActionResult> GetFilteredPagedUser([FromBody] GetTalentsList.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("position")]
        public async Task<IActionResult> AddJobPosition([FromBody] AddJobPosition.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("position")]
        public async Task<IActionResult> UpdateJobPosition([FromBody] UpdateJobPosition.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("positionStatus")]
        public async Task<IActionResult> UpdateJobPositionStatus([FromBody] UpdateJobPositionStatus.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("positions")]
        public async Task<IActionResult> GetJobPositions(GetJobPositions.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("positionById")]
        public async Task<IActionResult> GetJobPositionById(GetJobPositionById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("candidate")]
        public async Task<IActionResult> AddCandidateToJobPosition([FromBody] AddCandidateToJobPosition.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete("candidate")]
        public async Task<IActionResult> RemoveCandidateFromJobPosition(RemoveCandidateFromJobPosition.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("candidate/approve")]
        public async Task<IActionResult> ApproveCandidate([FromBody] ApproveCandidate.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("candidate/reject")]
        public async Task<IActionResult> RejectCandidate([FromBody] RejectCandidate.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("user/positions")]
        public async Task<IActionResult> GetUserJobPositions(GetUserJobPositions.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("jobs/list")]
        public async Task<IActionResult> GetFilteredPagedJobs([FromBody] GetJobsList.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("acceptJob")]
        public async Task<IActionResult> AcceptJobPosition([FromBody]AcceptJobPosition.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("user/applyTojob")]
        public async Task<IActionResult> ApplyToJob([FromBody]ApplyJobPosition.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("user/positionById")]
        public async Task<IActionResult> GetUserJobPositionById(GetUserJobPositionById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("user/jobNotifications")]
        public async Task<IActionResult> GetUserJobNotifications(GetUserJobNotificationsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
