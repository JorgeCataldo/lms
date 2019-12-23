using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Modules.Commands;
using Domain.Aggregates.Modules.Queries;
using Domain.Aggregates.Users;
using Infrastructure.ExcelServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ModuleController: BaseController
    {
        private readonly UserManager<User> _userManager;

        public ModuleController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }
        
        [HttpGet()]
        public async Task<IActionResult> GetAllPaged(GetPagedModulesQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("home")]
        public async Task<IActionResult> GetPagedHomeModules([FromBody]GetPagedHomeModulesQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered")]
        public async Task<IActionResult> GetAllPagedFiltered([FromBody]GetPagedModulesQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered/manager")]
        public async Task<IActionResult> GetManagerPagedFiltered([FromBody]GetPagedManagerModulesQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filtered/mycourses")]
        public async Task<IActionResult> GetMycoursesPagedFiltered([FromBody]GetPagedCoursesModulesQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetAllPaged(GetModuleSummaryByIdQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
           var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost()]
        public async Task<IActionResult> AddModule([FromBody] AddModuleCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateModule")]
        public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("deleteModule")]
        public async Task<IActionResult> DeleteModule([FromBody] DeleteModuleCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSubjects")]
        public async Task<IActionResult> ManageSubjects([FromBody] ManageSubjectsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageSupportMaterials")]
        public async Task<IActionResult> ManageSupportMaterials([FromBody] ManageSupportMaterialsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageContents")]
        public async Task<IActionResult> ManageContents([FromBody] ManageContentsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("manageRequirements")]
        public async Task<IActionResult> ManageRequirements([FromBody] ManageRequirementsCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("import")]
        public async Task<IActionResult> Import()
        {
            using (var fileStream = new FileStream(@"D:\GoogleDrive\tg4it\Clientes\Proseek\ASF\roteiros\BDQ BTG REV_TG_fil.xlsx", FileMode.Open, FileAccess.Read))
            {
                var a = new AcademyExcelService();
                var all = a.GenericWorkSheetData(fileStream);

                var modules = new List<AddModuleCommand.Contract>();
                foreach (var m in all.Modulos)
                {
                    var mod = new AddModuleCommand.Contract()
                    {
                        Excerpt = m.Ementa,
                        ImageUrl = "//www.imovelweb.com.br/noticias/wp-content/uploads/2015/11/concorrencia-1.jpg",
                        Instructor = "Felipe Gentil",
                        InstructorImageUrl = "//dev.academia.api.tg4.com.br/module/069e7088-ff07-4957-8604-0e6b382d10ee.png",
                        InstructorMiniBio = "CEO Proseek",
                        Published = true,
                        Duration = 200,
                        Title = m.DescricaoModulo,
                        Subjects = new List<AddModuleCommand.ContractSubject>(),
                        
                    };
                    foreach (var sub in m.Assuntos)
                    {
                        var assunto = new AddModuleCommand.ContractSubject()
                        {
                            Excerpt = sub.ObjetivoPedagogico,
                            Title = sub.DescricaoAssunto,
                            Concepts = sub.ConceitosLista.ToArray(),
                            Contents = new List<AddModuleCommand.ContractContent>()
                        };
                        mod.Subjects.Add(assunto);
                        foreach (var content in sub.Content)
                        {
                            var conteudo = new AddModuleCommand.ContractContent()
                            {
                                Concepts = content.ConceitosLista.ToArray(),
                                Excerpt = content.DescricaoConteudo,
                                Title = content.DescricaoConteudo,
                                Link = content.Link,
                                Type = content.Tipo == "VIDEO" ? ContentType.Video : content.Tipo == "PDF" ? ContentType.Pdf : ContentType.Text
                            };
                            assunto.Contents.Add(conteudo);
                        }
                    }
                    var result = await this.Send(mod);
                    if(result.IsFailure)
                        return RestResult.CreateHttpResponse(result);

                }
                return Ok();
            }
        }

        [HttpPut("setQuestionsLimit")]
        public async Task<IActionResult> SetQuestionsLimit([FromBody] SetQuestionsLimitCommand.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("effectivenessIndicators")]
        public async Task<IActionResult> GetEffectivenessIndicators([FromBody] GetEffectivenessIndicatorsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getAllContent")]
        public async Task<IActionResult> GetAllContent(GetAllContentQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetModuleOverviewQuery(GetModuleOverviewQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }
    }
}
