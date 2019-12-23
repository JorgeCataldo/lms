using Api.Extensions;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;
using Domain.Aggregates.GetAllLocations.Queries;

namespace Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LocationController : Controller
    {
        private readonly IMediator _mediator;
        private UserManager<User> _userManager;

        public LocationController(IMediator mediator, UserManager<User> userManager)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(GetAllLocations.Contract request)
        {
            
            var result = await _mediator.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
