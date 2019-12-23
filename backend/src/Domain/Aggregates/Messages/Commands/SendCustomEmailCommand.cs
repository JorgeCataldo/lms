using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Notifications;
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
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Messages.Commands
{
    public class SendCustomEmailCommand
    {
        public class Contract : CommandContract<Result<CustomEmail>>
        {
            public string UserId { get; set; }
            public List<string> UsersIds { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<CustomEmail>>
        {
            private readonly IDbContext _db;
            private readonly UserManager<User> _userManager;
            private readonly IEmailProvider _provider;
            private readonly IOptions<DomainOptions> _options;

            public Handler(IDbContext db, UserManager<User> userManager, IEmailProvider provider, IOptions<DomainOptions> options)
            {
                _db = db;
                _userManager = userManager;
                _provider = provider;
                _options = options;
            }

            public async Task<Result<CustomEmail>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var variables = new List<string>();
                var variablesToLower = new List<string>();

                if (String.IsNullOrEmpty(request.Text))
                    return Result.Fail<CustomEmail>("O Texto da Mensagem é obrigatório");
                if (ValidEmailVariables(request.Text, variables, variablesToLower))
                    return Result.Fail<CustomEmail>("Variáveis inválidas");
                if (request.UsersIds.Count == 0)
                    return Result.Fail<CustomEmail>("É necessario pelo menos um usuário para envio");

                var userId = ObjectId.Parse(request.UserId);

                var usersIds = request.UsersIds.Select(usId => ObjectId.Parse(usId)).ToList();

                var customEmailResult = CustomEmail.Create(userId, usersIds, request.Title, request.Text);
                if (customEmailResult.IsFailure)
                    return Result.Fail<CustomEmail>(customEmailResult.Error);

                var customEmail = customEmailResult.Data;

                foreach (ObjectId usrId in usersIds)
                {
                    await SendEmail(usrId, customEmail, variables, variablesToLower, cancellationToken);
                }
                    
                await _db.CustomEmailCollection.InsertOneAsync(customEmail);

                return Result.Ok(customEmail);
            }

            private bool ValidEmailVariables(string text, List<string> textVariables, List<string> textVariablestoLower)
            {
                var textVariablesCount = Regex.Matches(text, "--").Count;
                textVariablesCount = textVariablesCount > 0 ? textVariablesCount / 2 : 0;
                if (textVariablesCount == 0)                
                    return false;
                
                var index = 0;
                for (int i = 0; i < textVariablesCount; i++)
                {
                    var firstOccurrence = text.IndexOf("--", index);
                    index = firstOccurrence + 2;
                    var secondOccurrence = text.IndexOf("--", index);
                    index = secondOccurrence + 2;
                    var variable = text.Substring(firstOccurrence, index - firstOccurrence);
                    textVariables.Add(variable);
                    textVariablestoLower.Add(variable.ToLower());
                }

                var possibleVariables = CustomEmail.GetPossibleEmailVariables();
                possibleVariables = possibleVariables.ConvertAll(d => d.ToLower());

                return textVariablestoLower.Except(possibleVariables).Any();
            }

            private async Task<bool> SendEmail(ObjectId userId, CustomEmail message, 
                List<string> variables, List<string> variablesToLower, CancellationToken cancellationToken)
            {
                var user = await GetUser(userId, cancellationToken);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var text = GetEmailText(user, message.Text, variables, variablesToLower);

                    try
                    {
                        var emailData = new EmailUserData
                        {
                            Email = user.Email,
                            Name = user.Name,
                            ExtraData = new Dictionary<string, string> {
                                { "title", message.Title },
                                { "text", text }
                            }
                        };

                        await _provider.SendEmail(emailData, message.Title, "BTG-CustomEmail");
                        await SaveNotification(user.Id, message.Title, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return false;
            }

            private async Task<User> GetUser(ObjectId userId, CancellationToken token)
            {
                var userQuery = await _db.UserCollection.FindAsync(u =>
                    u.Id == userId,
                    cancellationToken: token
                );

                return await userQuery.SingleOrDefaultAsync(token);
            }

            private string GetEmailText(User user, string text, List<string> variables, List<string> variablesToLower)
            {
                for (int i = 0; i < variablesToLower.Count; i++)
                {
                    // Futuramente existirão mais variaveis disponiveis para mudanças
                    switch (variablesToLower[i])
                    {
                        case "--nome--":
                            text = text.Replace(variables[i], user.Name);
                            break;
                    }
                }
                return text;
            }

            private async Task<bool> SaveNotification(ObjectId userId, string title, bool emailDelivered)
            {
                var notification = Notification.Create(userId, emailDelivered, "Cheque seu e-mail!", "Lhe enviamos um email sobre: " + title);

                if (notification.IsSuccess)
                    await _db.NotificationCollection.InsertOneAsync(notification.Data);

                return notification.IsSuccess;
            }
        }
    }
}
