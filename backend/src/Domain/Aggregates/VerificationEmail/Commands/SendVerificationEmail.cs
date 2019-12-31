using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using Domain.IdentityStores.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.VerificationEmail.Commands
{
    public class SendVerificationEmail
    {
        public class Contract: CommandContract<Result<Contract>>
        {
            [Required]
            public string UserId { get; set; }
            public bool NewEmail { get; set; } = false;

        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;
            private readonly IEmailProvider _provider;

            public Handler(IDbContext db,
                IEmailProvider provider)
            {
                _db = db;
                _provider = provider;
            }
            
            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var userId = ObjectId.Parse(request.UserId);
                var user = await GetUser(userId, cancellationToken);
                if (user == null)
                    return Result.Fail<Contract>("Usuário não encontrado");

                var hasEmail = _db.UserVerificationEmailCollection.AsQueryable()
                        .Where(x => x.UserId == userId)
                        .FirstOrDefault();

                if (request.NewEmail || hasEmail == null)
                {
                    var userVerificationEmailResult = UserVerificationEmail.Create(userId);
                    if (userVerificationEmailResult.IsFailure)
                        return Result.Fail<Contract>(userVerificationEmailResult.Error);

                    var userVerificationEmail = userVerificationEmailResult.Data;

                    var emailSent = SendActivationCode(user, userVerificationEmail);

                    if (!emailSent.Result)
                        return Result.Fail<Contract>("Email não enviado");

                    await _db.UserVerificationEmailCollection.InsertOneAsync(userVerificationEmail);

                    return Result.Ok(request);
                }

                return Result.Ok(request);
            }

            private async Task<bool> SendActivationCode(User user, UserVerificationEmail email)
            {
                try
                {
                    var emailData = new EmailUserData
                    {
                        Email = user.Email,
                        Name = user.Name,
                        ExtraData = new Dictionary<string, string>
                        {
                            { "nome", user.Name },
                            { "code", email.Code }
                        }
                    };
                    await _provider.SendEmail(emailData, "Código de autenticação", "BTG-UserActivationCode");
                    return true;
                }
                catch (Exception err)
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
        }
    }
}