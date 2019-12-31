﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.ModulesDrafts.Commands;
using Domain.Aggregates.ModulesDrafts.Queries;
using Domain.Aggregates.Users;
using Domain.ExternalService;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ModuleDraftController : BaseController
    {
        private readonly UserManager<User> _userManager;
		private readonly IConfiguration _configuration;
		
        public ModuleDraftController(IMediator mediator, UserManager<User> userManager, IConfiguration configuration) : base(mediator)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedModulesAndDrafts([FromBody]GetPagedModulesAndDraftsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetModuleDraftById(GetModuleDraftById.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost()]
        public async Task<IActionResult> AddModule([FromBody] AddModuleDraftCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPost("clone")]
        public async Task<IActionResult> CloneModule([FromBody] CloneModuleDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateModule")]
        public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleDraft.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("deleteModule")]
        public async Task<IActionResult> DeleteModule([FromBody] DeleteModuleDraftCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSubjects")]
        public async Task<IActionResult> ManageSubjects([FromBody] ManageDraftSubjects.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSupportMaterials")]
        public async Task<IActionResult> ManageSupportMaterials([FromBody] ManageDraftSupportMaterialsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageContents")]
        public async Task<IActionResult> ManageContents([FromBody] ManageDraftContents.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageRequirements")]
        public async Task<IActionResult> ManageRequirements([FromBody] ManageDraftRequirementsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("publishDraft")]
        public async Task<IActionResult> PublishDraft(PublishModuleDraftCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("setQuestionsLimit")]
        public async Task<IActionResult> SetDraftQuestionsLimit([FromBody] SetDraftQuestionsLimit.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
        
        [HttpPut("manageEcommerceModuleDraft")]
        public async Task<IActionResult> ManageEcommerceProducts([FromBody] ManageEcommerceDraft.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageModuleWeight")]
        public async Task<IActionResult> ManageDraftModuleWeight([FromBody] ManageModuleWeightDraft.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("sendFileToS3"), DisableRequestSizeLimit]
        public async Task<IActionResult> SendFileToS3()
        {
            try
            {
                if (Request.Form == null)
                {
                    throw new Exception("Arquivo não encontrado");
                }

                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    var result = await AWS.UploadFileToS3(file, _configuration);
                    return RestResult.CreateHttpResponse(result);
                }

                return RestResult.CreateFailHttpResponse("Ocorreu um erro ao realizar a operação.");
            }
            catch (Exception ex)
            {
                var execption = ex.Message;
                return RestResult.CreateFailHttpResponse("Ocorreu um erro ao realizar a operação.");
            }
        }
    }
}
