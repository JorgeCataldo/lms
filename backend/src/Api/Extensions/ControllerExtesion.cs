using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions
{
    public static class ControllerExtension
    {              
        public static string GetUserId(this Controller controller)
        {
            try
            {
                var userId = controller.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                var impersonateId = controller.User.Claims.FirstOrDefault(x => x.Type == "UserIdToImpersonate")?.Value;

                return impersonateId ?? userId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetUserRole(this Controller controller)
        {
            try
            {
                var userRole = controller.User.Claims.First(x => x.Type == ClaimTypes.Role).Value;

                var impersonateRole = controller.User.Claims.FirstOrDefault(x => x.Type == "UserRoleToImpersonate")?.Value;

                var seeHowRole = controller.User.Claims.FirstOrDefault(x => x.Type == "UserRoleToSeeHow")?.Value;

                if (impersonateRole != null)
                {
                    return impersonateRole;
                }
                else if (seeHowRole != null)
                {
                    return seeHowRole;
                }
                return userRole;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}