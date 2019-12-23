using System.Threading.Tasks;
using Domain.Aggregates.Report.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;
using Api.Extensions;
using Microsoft.AspNetCore.Identity;
using Domain.Aggregates.Users;
using System;
using System.IO;
using Domain.Aggregates.Users.Queries;
using Domain.Aggregates.UsersCareer.Queries;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReportController: Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public ReportController(IMediator mediator, UserManager<User> userManager)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        [HttpGet("getTrackReportStudents")]
        public async Task<IActionResult> GetTrackReportStudents(GetTrackReportStudentsQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("effectivenessIndicators")]
        public async Task<IActionResult> GetEffectivenessIndicators(GetEffectivenessIndicatorsQuery.Contract request)
        {

            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("userProgress")]
        public async Task<IActionResult> GetuserProgress(GetUsersPerfomanceTimeReportQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("exportCareerUsers")]
        public async Task<IActionResult> ExportUsersCareer(ExportCareerQuery.Contract request)
        {

            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("trackGrades")]
        public async Task<IActionResult> ExportTrackGrades(GetTracksGradesQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getTrackNps")]
        public async Task<IActionResult> ExportTrackNps(GetAllNpsQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpGet("getTrackAnswers")]
        public async Task<IActionResult> GetTrackAnswers(GetAnswersQuery.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getAtypicalMovements")]
        public async Task<IActionResult> GetAtypicalMovements(GetAtypicalMovementsQuery.Contract request)
        {
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getFinanceReport")]
        public async Task<IActionResult> GetFinanceReport(GetFinanceReportQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
