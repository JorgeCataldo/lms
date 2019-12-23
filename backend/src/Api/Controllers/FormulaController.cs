using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Formulas.Commands;
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
    public class FormulaController : BaseController
    {
        private UserManager<User> _userManager;

        public FormulaController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetFormulas(GetFormulas.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetFormulaById(GetFormulaById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddFormula([FromBody] AddFormula.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manage")]
        public async Task<IActionResult> ManageFormula([FromBody] ManageFormula.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFormula([FromBody] DeleteFormula.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("variables/byType")]
        public async Task<IActionResult> GetFormulaTypeVariables(GetFormulaTypeVariables.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageTypeVariables")]
        public async Task<IActionResult> ManageFormulaTypeVariables([FromBody] ManageFormulaTypeVariables.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}