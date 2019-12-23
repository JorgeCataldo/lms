using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Responsibles.Commands;
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
    public class ResponsibleController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public ResponsibleController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost("createResponsibleTree")]
        public async Task<IActionResult> CreateResponsibleTree([FromBody]GenerateResponsibleTreeCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
