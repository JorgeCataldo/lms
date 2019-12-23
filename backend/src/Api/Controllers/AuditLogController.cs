using Domain.Aggregates.Users;
using Domain.Aggregates.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tg4.Infrastructure.Web.Message;
using Api.Extensions;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuditLogController : Controller
    {

        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public AuditLogController(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet("allLogs")]
        public async Task<IActionResult> GetAll(GetAllQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("allQuestions")]
        public async Task<IActionResult> GetAllUpdatedQuestionsDraft([FromBody]GetUpdatedQuestionsDraftQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedLog([FromBody]GetPagedUserSyncProcesseQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }

}

