using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Base;
using Domain.Aggregates.Levels;
using Domain.Aggregates.Modules.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LevelController : BaseController
    {
        public LevelController(IMediator mediator): base(mediator)
        {
        }
        
        [HttpGet()]
        public IActionResult Get(bool GetAll)
        {
            var result = GetAll ? Level.GetAllLevels() : Level.GetLevels();
            return RestResult.CreateHttpResponse(result);
        }
    }
}
