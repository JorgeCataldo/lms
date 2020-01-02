using System.Threading.Tasks;
using Api.Extensions;
using Domain.Aggregates.Events.Queries;
using Domain.Aggregates.Forums.Queries;
using Domain.Aggregates.Messages.Commands;
using Domain.Aggregates.Notifications.Commands;
using Domain.Aggregates.Notifications.Queries;
using Domain.Aggregates.UserFiles.Commands;
using Domain.Aggregates.UserContentNotes.Commands;
using Domain.Aggregates.UserContentNotes.Queries;
using Domain.Aggregates.UserProgressHistory.Queries;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Commands;
using Domain.Aggregates.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tg4.Infrastructure.Web.Message;
using Domain.Aggregates.UsersCareer.Queries;
using Domain.Aggregates.UsersCareer.Commands;
using Domain.Aggregates.Files.Commands;
using System.IO;
using System;
using Domain.Aggregates.ColorPalletes.Queries;
using Domain.Aggregates.ColorPalletes.Commands;
using Domain.Aggregates.NetPromoterScores.Queries;
using Domain.Aggregates.NetPromoterScores.Commands;
using Api.Base;
using MongoDB.Bson;
using Infrastructure.ExcelServices.Extensions;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UserController: BaseController
    {
        private UserManager<User> _userManager;

        public UserController(IMediator mediator, UserManager<User> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpPost()]
        public async Task<IActionResult> GetById([FromBody] GetUserByIdQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("userProfile")]
        public async Task<IActionResult> GetProfileById([FromBody] GetUserProfileQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("pagedUser")]
        public async Task<IActionResult> GetPagedUser([FromBody] GetPagedUserQuery.Contract request)
        {
            request.UserRole = this.GetUserRole();
            request.UserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("filteredPagedUser")]
        public async Task<IActionResult> GetFilteredPagedUser([FromBody] GetFilteredPagedUserQuery.Contract request)
        {   
            request.UserRole = this.GetUserRole();
            request.UserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("changeUserBlockedStatus")]
        public async Task<IActionResult> ChangeUserBlockedStatus([FromBody] BlockUser.Contract request)
        {
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("createUpdateUser")]
        public async Task<IActionResult> CreateUpdateUser([FromBody] AddOrModifyUserCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserCategories")]
        public async Task<IActionResult> GetUserCategory(GetUserCategoriesQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("pagedUsersSyncProcesse")]
        public async Task<IActionResult> GetPagedUsersSyncProcesse([FromBody] GetPagedUserSyncProcesseQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getmodulesprogress")]
        public async Task<IActionResult> GetUserModulesProgress(GetUserModuleProgressQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("gettracksprogress")]
        public async Task<IActionResult> GetUserTracksProgress(GetUserTracksProgressQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getTrackProgress")]
        public async Task<IActionResult> GetUserTrackProgress(GetUserTrackProgressQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getmoduleprogress")]
        public async Task<IActionResult> GetUserModuleProgress(GetUserModuleSubjectsProgressQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getsubjectprogress")]
        public async Task<IActionResult> GetUserSubjectProgress(GetUserSubjectProgress.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("geteventapplicationstatus")]
        public async Task<IActionResult> GetEventApplicationByUser(GetEventApplicationByUserQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateUserRecommendations")]
        public async Task<IActionResult> UpdateUserRecommendations([FromBody] UpdateRecommendationsCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getProfessors")]
        public async Task<IActionResult> GetProfessors(GetProfessorsQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getNotifications")]
        public async Task<IActionResult> GetNotifications(GetUserNotificationsQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageNotification")]
        public async Task<IActionResult> ManageNotification([FromBody] ManageNotificationCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        
        [HttpPost("testemails")]
        public async Task<IActionResult> TestEmails([FromBody] TestEmailsCommand.Contract request)
        {
            var email = "";
            var role = this.GetUserRole();
            if (role != "Admin")
                return this.Unauthorized();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getContactAreas")]
        public async Task<IActionResult> GetAreas(GetContactAreasQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("blockUserMaterial")]
        public async Task<IActionResult> BlockUserMaterial([FromBody] BlockUserMaterial.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("sendCustomEmail")]
        public async Task<IActionResult> SendCustomEmail([FromBody] SendCustomEmailCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("pagedCustomEmails")]
        public async Task<IActionResult> GetPagedCustomEmails([FromBody] GetPagedCustomEmailsQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserFiles")]
        public async Task<IActionResult> GetUserFiles(GetUserFilesQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpPost("addUserFiles")]
        public async Task<IActionResult> ManageUserFiles([FromBody] AddUserFilesCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("addAssesmentUserFiles")]
        public async Task<IActionResult> ManageAssesmentUserFiles([FromBody] AddAssesmentUserFilesCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("manageUserFiles")]
        public async Task<IActionResult> ManageUserFiles([FromBody] ManageUserFilesCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = ObjectId.Parse(this.GetUserId());
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserContentNote")]
        public async Task<IActionResult> GetUserContentNote(GetUserContentNoteQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateUserContentNote")]
        public async Task<IActionResult> UpdateUserContentNote([FromBody] AddOrModifyUserContentNoteCommand.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserCareer")]
        public async Task<IActionResult> GetUserCareer(GetUserCareerQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateUserCareer")]
        public async Task<IActionResult> UpdateUserCareer([FromBody] UpdateUserCareerCommand.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserInstitutions")]
        public async Task<IActionResult> GetUserInstitutions(GetUserInstitutionsQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("exportUsersCareer")]
        public async Task<IActionResult> ExportUsersCareer([FromBody] ExportUsersCareerQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("exportUsersGrade")]
        public async Task<IActionResult> ExportUsersGrade([FromBody] ExportUsersGradeQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("userRecommendationBasicInfo")]
        public async Task<IActionResult> GetUserRecommendation(GetUserRecommendationQuery.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserSkills")]
        public async Task<IActionResult> GetUserSkills(GetUserSkillsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("allowRecommendation")]
        public async Task<IActionResult> ManageAllowUserRecommendation([FromBody] ManageAllowUserRecommendation.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.CurrentUserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("AddUsersSyncProcess")]
        public async Task<IActionResult> AddUsersSyncProcess([FromBody] AddFileCommand.Contract request)
        {
            
            var role = this.GetUserRole();
            if(role != "Admin" && role != "HumanResources" && role != "Secretary" && role != "Recruiter")
                return this.Unauthorized();

            request.FileContent = request.FileContent.Substring(request.FileContent.IndexOf(",", StringComparison.Ordinal) + 1);
            Byte[] bytes = Convert.FromBase64String(request.FileContent);
            using (Stream stream = new MemoryStream(bytes))
            {
                StreamReader reader = new StreamReader( stream );
                request.FileContent = reader.ReadToEnd();
            }

            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPut("secretary/allowRecommendation")]
        public async Task<IActionResult> ManageAllowUserRecommendation([FromBody] ManageAllowSecretaryRecommendation.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("AutomaticSyncProcess")]
        [AllowAnonymous]
        public async Task<IActionResult> AutomaticSyncProcess([FromBody] AddFileCommand.Contract request)
        {
            if(string.IsNullOrEmpty(request.SyncAppId))
                return this.Unauthorized();

            request.FileContent = request.FileContent.Substring(request.FileContent.IndexOf(",", StringComparison.Ordinal) + 1);
            Byte[] bytes = Convert.FromBase64String(request.FileContent);
            using (Stream stream = new MemoryStream(bytes))
            {
                StreamReader reader = new StreamReader( stream );
                request.FileContent = reader.ReadToEnd();
            }

            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("GetCEP")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCEP(GetCEPQuery.Contract request)
        {
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserColorPalette")]
        public async Task<IActionResult> GetUserColorPalette(GetUserColorPalette.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("updateUserColorPalette")]
        public async Task<IActionResult> UpdateUserColorPalette([FromBody] UpdateUserColorPalette.Contract request)
        {
            
            request.CurrentUserRole = this.GetUserRole();
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("getFilteredNps")]
        public async Task<IActionResult> GetFilteredNps([FromBody] GetFilteredNpsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("saveNps")]
        public async Task<IActionResult> SaveNps([FromBody] SaveNpsCommand.Contract request)
        {
            
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getAllNps")]
        public async Task<IActionResult> GetAllNps(GetAllNpsQuery.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("GetUserNpsAvailability")]
        public async Task<IActionResult> GetUserNpsAvailability(GetUserNpsAvailabilityQuery.Contract request)
        {
            
            request.UserId = this.GetUserId();
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getBasicProgressInfo")]
        public async Task<IActionResult> GetUserBasicProgressInfo(GetUserBasicProgressInfoQuery.Contract request)
        {
            request.UserId = this.GetUserId();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpGet("getUserToImpersonate")]
        public async Task<IActionResult> GetUserToImpersonate(GetUserToImpersonate.Contract request)
        {
            
            request.UserRole = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }

        [HttpPost("SeeHow")]
        public async Task<IActionResult> SeeHow([FromBody] SetSeeHowCommand.Contract request)
        {
            var user = await _userManager.GetUserAsync(User);
            var role = user?.GetUserRole(user);
            if (role != "Admin")
                return this.Unauthorized();
            request.UserRoleToChange = this.GetUserRole();
            var result = await this.Send(request);
            return RestResult.CreateHttpResponse(result);
        }


        [HttpPost("exportUsersToSuperlogica")]
        public async Task<IActionResult> GetAnswers([FromBody]ExportUsersToSuperlogica.Contract request)
        {
            
            var result = await this.Send(request);
            if (result.IsFailure)
                return RestResult.CreateHttpResponse(result);

            var bytes = result.Data.ToExcel("users");
            return File(new MemoryStream(bytes), "application/octet-stream", "answers.xlsx");
        }
    }
}
