using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Commands;
using Domain.ECommerceIntegration.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class EcommerceController : BaseController
    {
        private UserManager<User> _userManager;

        public EcommerceController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost()]
        public async Task<IActionResult> ProductUpdated([FromBody] AddUserByOrderCommand.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("migrateStructure")]
        public async Task<IActionResult> MigrateStructure(MigrateStructure.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
