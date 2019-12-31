using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Data;
using Domain.Base;
using Domain.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Email.Services;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Messages.Commands
{
    public class SendMessageCommand
    {
        public class Contract : CommandContract<Result<Message>>
        {
            public string AreaId { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public string FileUrl { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Message>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;
            private readonly IEmailProvider _provider;
            private readonly IOptions<DomainOptions> _options;
            private readonly IOptions<SendGridOptions> _sendGridOptions;

            public Handler(IDbContext db, UserManager<User> userManager, IEmailProvider provider, 
                IOptions<DomainOptions> options, IOptions<SendGridOptions> sendGridOptions)
            {
                _db = db;
                _userManager = userManager;
                _provider = provider;
                _options = options;
                _sendGridOptions = sendGridOptions;
            }

            public async Task<Result<Message>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    if (String.IsNullOrEmpty(request.AreaId))
                        return Result.Fail<Message>("A Área de Contato é obrigatória");
                    if (String.IsNullOrEmpty(request.Title))
                        return Result.Fail<Message>("O Título da Mensagem é obrigatório");
                    if (String.IsNullOrEmpty(request.Text))
                        return Result.Fail<Message>("O Texto da Mensagem é obrigatório");

                    var contactAreaId = ObjectId.Parse(request.AreaId);
                    var userId = ObjectId.Parse(request.UserId);

                    var contactArea = await GetContactArea(contactAreaId, cancellationToken);
                    if (contactArea == null)
                        return Result.Fail<Message>("Área de Contato não existe");

                    var messageResult = Message.Create(userId, contactAreaId, request.Title, request.Text, request.FileUrl);
                    if (messageResult.IsFailure)
                        return Result.Fail<Message>(messageResult.Error);

                    var message = messageResult.Data;

                    await SendEmail(userId, contactArea, message, cancellationToken);
                    await _db.MessageCollection.InsertOneAsync(message);

                    return Result.Ok(message);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    throw err;
                }
            }

            private async Task<bool> SendEmail(ObjectId userId, ContactArea area, Message message, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await GetUser(userId, cancellationToken);

                    /*var emailData = new EmailUserData {
                        Email = area.Email,
                        Name = user.Name,
                        ExtraData = new Dictionary<string, string> {
                            { "contactArea", area.Description },
                            { "userName", user.UserName },
                            { "userEmail", user.Email },
                            { "title", message.Title },
                            { "text", message.Text },
                            { "fileUrl", message.FileUrl }
                        }
                    };

                    var template = string.IsNullOrEmpty(message.FileUrl) ? "BTG-CostumerSupportMessage" : "BTG-CostumerSupportMessageFile";

                    await _provider.SendEmail(
                        emailData,
                        "Academia - " + "[" + area.Description + "]" + " - " + message.Title,
                        template
                    );*/

                    var templateId = string.IsNullOrEmpty(message.FileUrl) ? "775bc342-9028-43db-9afc-275c1d878bc2" : "07d4f0cf-044e-4bcb-98c8-b3502bf01ad6";

                    var client = new SendGridClient(_sendGridOptions.Value.SendGridKey);
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(_sendGridOptions.Value.Domain, "Contato"),
                        Subject = "Academia - " + "[" + area.Description + "]" + " - " + message.Title,
                        TemplateId = templateId,
                        Personalizations = new List<Personalization>
                        {
                             new Personalization
                             {
                                Tos = new List<EmailAddress>
                                {
                                    new EmailAddress(user.Email, user.UserName),
                                    new EmailAddress(area.Email, "Contato")
                                },
                                Substitutions = new Dictionary<string, string> {
                                    { "-contactArea-", area.Description },
                                    { "-userName-", user.UserName },
                                    { "-userEmail-", user.Email },
                                    { "-title-", message.Title },
                                    { "-text-", message.Text },
                                    { "-fileUrl-", message.FileUrl }
                                }
                             }
                        }
                    };

                    var response = await client.SendEmailAsync(msg);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                var userQuery = await _db.UserCollection.FindAsync(u =>
                    u.Id == userId,
                    cancellationToken: token
                );

                return await userQuery.SingleOrDefaultAsync(token);
            }

            private async Task<ContactArea> GetContactArea(ObjectId areaId, CancellationToken token)
            {
                var query = await _db.ContactAreaCollection.FindAsync(u =>
                    u.Id == areaId,
                    cancellationToken: token
                );

                return await query.SingleOrDefaultAsync(token);
            }
        }
    }
}
