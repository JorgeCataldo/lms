using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Api.Base;
using Api.Extensions;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Tg4.Infrastructure.Web.Message;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ImportsController : BaseController
    {
        private UserManager<User> _userManager;

        public ImportsController(IMediator mediator, UserManager<User> userManager): base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet("userDatabase")]
        public async Task<IActionResult> ImportUserDatabase()
        {
            
            var role = this.GetUserRole();

            if (role == "Student" || role == "Secretary" || role == "Recruiter")
                return Unauthorized();

            using (
                var fileStream = new FileStream(
                    @"C:\Users\leona\Documents\Programming\btg\backend\src\Api\IMPORT_BD_LMS.xlsx",
                    FileMode.Open, FileAccess.Read
                )
            ) {

                using (var xlPackage = new ExcelPackage(fileStream))
                {
                    try
                    {
                        Console.WriteLine("Lendo Planilha... " + DateTimeOffset.Now.ToString("hh:mm:ss.fff tt"));
                        var baseWorksheet1 = xlPackage.Workbook.Worksheets[0];
                        var usersInfo = baseWorksheet1.ConvertSheetToObjects<UserInfoDB>();

                        var contract = new UpdateUsersByImport.Contract() {
                            UsersInfos = usersInfo.ToList()
                        };

                        var result = await this.Send( contract );

                        if (result.IsFailure)
                            return RestResult.CreateHttpResponse(result);
                    }
                    catch (Exception error)
                    {
                        var err = error;
                        return BadRequest();
                    }
                }

                return Ok();
            }
        }
    }
}

public static class EPPLusExtensions
{
    public static IEnumerable<T> ConvertSheetToObjects<T>(this ExcelWorksheet worksheet) where T : new()
    {
        Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(Column);

        var columns = typeof(T)
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(columnOnly))
            .Select(p => new {
                Property = p,
                Column = p.GetCustomAttributes<Column>().First().ColumnIndex //safe because if where above
            }).ToList();

        var rows = worksheet.Cells
            .Select(cell => cell.Start.Row)
            .Distinct()
            .OrderBy(x => x);
        
        var collection = rows.Skip(1)
            .Select(row => {
                var tnew = new T();
                columns.ForEach(col => {
                    var val = worksheet.Cells[row, col.Column];

                    if (val.Value == null)
                    {
                        col.Property.SetValue(tnew, null);
                        return;
                    }
                    if (col.Property.PropertyType == typeof(Int32))
                    {
                        col.Property.SetValue(tnew, val.GetValue<int>());
                        return;
                    }
                    if (col.Property.PropertyType == typeof(double))
                    {
                        col.Property.SetValue(tnew, val.GetValue<double>());
                        return;
                    }
                    if (col.Property.PropertyType == typeof(DateTime))
                    {
                        col.Property.SetValue(tnew, val.GetValue<DateTime>());
                        return;
                    }
                    col.Property.SetValue(tnew, val.GetValue<string>());
                });

                return tnew;
            });

        return collection;
    }
}