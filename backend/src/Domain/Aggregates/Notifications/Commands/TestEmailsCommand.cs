using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using Domain.Base;
using Domain.IdentityStores.Settings;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Notifications.Commands
{
    public class TestEmailsCommand
    {
        public class Contract : CommandContract<Result>
        {
            public string email { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly IEmailProvider _provider;
            private readonly IConfiguration _configuration;

            Dictionary<string, EmailUserData> emails = new Dictionary<string, EmailUserData>();

            public Handler(IEmailProvider provider, IConfiguration configuration)
            {
                _provider = provider;
                _configuration = configuration;

            }

            private void BuildTemplates()
            {

                //emails.Add("BTG-ForgotPasswordTemplate", new EmailUserData
                //{
                //    Email = "test User",
                //    Name = "test Name",
                //    ExtraData = new Dictionary<string, string>{
                //        { "name", "test Name" },
                //        { "token", "test Token" },
                //        { "siteurl", "http://www.tg4.com.br" }
                //    }
                //});
                //emails.Add("BTG-EventStudentValuation", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string>
                //            {
                //                { "name", "Test UserName" },
                //                { "eventTitle", "Test Event" },
                //                { "valuateUrl", "http://www.tg4.com.br/" }
                //            }
                //});
                //emails.Add("BTG-ForumQuestionAnswered", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string>
                //            {
                //                { "name", "Test UserName" },
                //                { "forumTitle", "Test Title" },
                //                { "forumUrl", "http://www.tg4.com.br/" }
                //            }
                //});
                //emails.Add("BTG-ForumQuestionInstructor", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string>
                //                {
                //                    { "name", "Test UserName" },
                //                    { "forumTitle", "Test Forum Title" },
                //                    { "moduleName", "Test Module Title" },
                //                    { "forumUrl", "http://www.tg4.com.br/" }
                //                }
                //});
                //emails.Add("BTG-CustomEmail", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string> {
                //                { "title", "Test Title" },
                //                { "text", "Test Text" }
                //            }
                //});
                //emails.Add("BTG-CostumerSupportMessageFile", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string> {
                //            { "contactArea", "Description" },
                //            { "userName", "Test UserName" },
                //            { "userEmail", "webmaster@tg4.com.br" },
                //            { "title", "message.Title" },
                //            { "text", "message.Text" },
                //            { "fileUrl", "message.FileUrl" }
                //        }
                //});
                //emails.Add("BTG-CostumerSupportMessage", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string> {
                //            { "contactArea", "Description" },
                //            { "userName", "Test UserName" },
                //            { "userEmail", "webmaster@tg4.com.br" },
                //            { "title", "message.Title" },
                //            { "text", "message.Text" },
                //            { "fileUrl", "message.FileUrl" }
                //        }
                //});
                emails.Add("BTG-NewUserMessage", new EmailUserData
                {
                    Email = "webmaster@tg4.com.br",
                    Name = "Test Name",
                    ExtraData = new Dictionary<string, string> {
                            { "name", "Test Name" },
                            { "token", "password" },
                            { "username", "username"},
                            { "siteurl", _configuration[$"DomainOptions:SiteUrl"] },
                            { "studentmanual", _configuration[$"DomainOptions:StudentManual"] },
                            { "platformtutorial", _configuration[$"DomainOptions:PlatformTutorial"] }
                        }
                });
                emails.Add("BTG-UserActivationCode", new EmailUserData
                {
                    Email = "webmaster@tg4.com.br",
                    Name = "Test Name",
                    ExtraData = new Dictionary<string, string>
                        {
                            { "nome", "Test Name" },
                            { "code", "email.Code" }
                        }
                });
                //emails.Add("BTG-UserActivationCode", new EmailUserData
                //{
                //    Email = "webmaster@tg4.com.br",
                //    Name = "Test Name",
                //    ExtraData = new Dictionary<string, string>
                //        {
                //            { "nome", "Test Name" },
                //            { "code", "email.Code" }
                //        }
                //});
            }
            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                BuildTemplates();
                foreach (var key in this.emails.Keys)
                {
                    var emailData = this.emails[key];
                    emailData.Email = request.email;

                    await _provider.SendEmail(emailData, $"Test {key}", key);
                }
                return Result.Ok();
            }
        }
    }
}