using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Responsibles;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Users.Commands
{
    public class AddOrModifyUserCommand
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string RegistrationId { get; set; }
            public string Cpf { get; set; }
            public string Password { get; set; }
            public string Responsible { get; set; }
            public string LineManager { get; set; }
            public string Info { get; set; }
            public string Phone { get; set; }
            public string Phone2 { get; set; }
            public string ImageUrl { get; set; }
            public RelationalItem BusinessGroup { get; set; }
            public RelationalItem BusinessUnit { get; set; }
            public RelationalItem Country { get; set; }
            public RelationalItem FrontBack { get; set; }
            public RelationalItem Job { get; set; }
            public RelationalItem Location { get; set; }
            public RelationalItem Rank { get; set; }
            public List<RelationalItem> Sector { get; set; }
            public RelationalItem Segment { get; set; }
            public string Role { get; set; }
            public string CurrentUserRole { get; set; }
            public ObjectId CurrentUserId { get; set; }
            public Address Address { get; set; }
            public int? Document { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentEmitter { get; set; }
            public DateTimeOffset? EmitDate { get; set; }
            public DateTimeOffset? ExpirationDate { get; set; }
            public bool? SpecialNeeds { get; set; }
            public string SpecialNeedsDescription { get; set; }
            public string LinkedIn { get; set; }
            public bool? ForumActivities { get; set; }
            public string ForumEmail { get; set; }
            public DateTimeOffset? DateBorn { get; set; }
            // Configurável pelo App
            public bool AutoGenerateRegistrationId { get; set; }
        }

        public class RelationalItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;

            public Handler(
                IDbContext db,
                UserManager<User> userManager, 
                IMediator mediator,
                IConfiguration configuration
            ) {
                _db = db;
                _userManager = userManager;
                _mediator = mediator;
                _configuration = configuration;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                string autoGenerateRegistrationId = _configuration[$"Permissions:AutoGenerateRegistrationId"];

                var userResult = User.CreateUser(
                    request.Name,
                    request.Email, 
                    request.UserName,
                    request.RegistrationId,
                    request.Cpf, 
                    request.Responsible, 
                    request.LineManager,
                    request.Info, 
                    request.Phone, 
                    request.ImageUrl, 
                    request.Role, 
                    request.BusinessGroup == null ? null : new User.RelationalItem(ObjectId.Parse(request.BusinessGroup.Id), request.BusinessGroup.Name),
                    request.BusinessUnit == null ? null : new User.RelationalItem(ObjectId.Parse(request.BusinessUnit.Id), request.BusinessUnit.Name),
                    request.Country == null ? null : new User.RelationalItem(ObjectId.Parse(request.Country.Id), request.Country.Name),
                    request.FrontBack == null ? null : new User.RelationalItem(ObjectId.Parse(request.FrontBack.Id), request.FrontBack.Name),
                    request.Job == null ? null : new User.RelationalItem(ObjectId.Parse(request.Job.Id), request.Job.Name), 
                    request.Location == null ? null : new User.RelationalItem(ObjectId.Parse(request.Location.Id), request.Location.Name),
                    request.Rank == null ? null : new User.RelationalItem(ObjectId.Parse(request.Rank.Id), request.Rank.Name),
                    request.Sector.Where(x => x != null).Select(x => new User.RelationalItem(ObjectId.Parse(x.Id), x.Name)).ToList(),
                    request.Segment == null ? null : new User.RelationalItem(ObjectId.Parse(request.Segment.Id), request.Segment.Name)
                );
                
                if (userResult.IsFailure)
                    return Result.Fail<Contract>(userResult.Error);
                
                var newUser = userResult.Data;
                newUser.Address = request.Address;
                if (request.Document.HasValue)
                    newUser.Document = (DocumentType)request.Document.Value;
                newUser.DocumentNumber = request.DocumentNumber;
                newUser.DocumentEmitter = request.DocumentEmitter;
                newUser.EmitDate = request.EmitDate;
                newUser.ExpirationDate = request.ExpirationDate;
                newUser.SpecialNeeds = request.SpecialNeeds;
                newUser.SpecialNeedsDescription = request.SpecialNeedsDescription;
                newUser.LinkedIn = request.LinkedIn;
                newUser.ForumActivities = request.ForumActivities;
                newUser.ForumEmail = request.ForumEmail;
                newUser.DateBorn = request.DateBorn;

                if (string.IsNullOrEmpty(request.Id))
                {
                    if (request.CurrentUserRole == "Secretary" || request.CurrentUserRole == "Student" || request.CurrentUserRole == "Recruiter")
                        return Result.Fail<Contract>("Acesso Negado");

                    if (autoGenerateRegistrationId == "True")
                        newUser.RegistrationId = await GetNextRegistrationId(cancellationToken);

                    var result = await _userManager.CreateAsync(newUser, request.Password);

                    if (!result.Succeeded)
                    {
                        var error = result.Errors.FirstOrDefault();
                        return error != null ? Result.Fail<Contract>(error.Description) : Result.Fail<Contract>("Erro interno ao criar usuário");
                    }
                }
                else
                {
                    var query = await GetQueryByRole(
                        request.Role,
                        ObjectId.Parse(request.Id),
                        cancellationToken
                    );

                    var user = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (user == null)
                        return Result.Fail<Contract>("Usuário não Encontrado");
                    
                    if (request.CurrentUserRole == "Student" &&
                        user.Id != request.CurrentUserId &&
                        user.ResponsibleId != request.CurrentUserId)
                        return Result.Fail<Contract>("Acesso Negado");

                    user.Name = newUser.Name;
                    user.Info = newUser.Info;
                    user.ImageUrl = newUser.ImageUrl;
                    user.Address = newUser.Address;
                    user.Document = newUser.Document;
                    user.DocumentNumber = newUser.DocumentNumber;
                    user.DocumentEmitter = newUser.DocumentEmitter;
                    user.EmitDate = newUser.EmitDate;
                    user.ExpirationDate = newUser.ExpirationDate;
                    user.SpecialNeeds = newUser.SpecialNeeds;
                    user.SpecialNeedsDescription = newUser.SpecialNeedsDescription;
                    user.LinkedIn = newUser.LinkedIn;
                    user.ForumActivities = newUser.ForumActivities;
                    user.ForumEmail = newUser.ForumEmail;
                    user.DateBorn = newUser.DateBorn;
                    user.Phone = newUser.Phone;

                    if (request.CurrentUserRole != "Student")
                    {
                        user.Email = newUser.Email;
                        user.RegistrationId = newUser.RegistrationId;
                        user.Cpf = newUser.Cpf;
                        user.ResponsibleId = newUser.ResponsibleId;
                        user.LineManager = newUser.LineManager;
                        user.BusinessGroup = newUser.BusinessGroup;
                        user.BusinessUnit = newUser.BusinessUnit;
                        user.Country = newUser.Country;
                        user.FrontBackOffice = newUser.FrontBackOffice;
                        user.Job = newUser.Job;
                        user.Location = newUser.Location;
                        user.Rank = newUser.Rank;
                        user.Sectors = newUser.Sectors;
                        user.Segment = newUser.Segment;
                    }

                    if (newUser.NormalizedUsername != user.NormalizedUsername)
                    {
                        var hasUsername = await _db.UserCollection
                            .AsQueryable()
                            .FirstOrDefaultAsync(x => x.UserName == newUser.UserName);

                        if (hasUsername != null)
                            return Result.Fail<Contract>("Nome de Usuário indisponível");

                        user.UserName = newUser.UserName;
                        user.NormalizedUsername = newUser.NormalizedUsername;
                    }

                    await _db.UserCollection.ReplaceOneAsync(t =>
                        t.Id == user.Id, user,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(request);
            }

            private async Task<IAsyncCursor<User>> GetQueryByRole(string Role, ObjectId userId, CancellationToken token)
            {
                switch (Role)
                {
                    case "Admin":
                        return await _db.Database
                            .GetCollection<Admin>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "HumanResources":
                        return await _db.Database
                            .GetCollection<HumanResources>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "Secretary":
                        return await _db.Database
                            .GetCollection<Secretary>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "Recruiter":
                        return await _db.Database
                            .GetCollection<Recruiter>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "BusinessManager":
                        return await _db.Database
                            .GetCollection<BusinessManager>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "Author":
                        return await _db.Database
                            .GetCollection<Author>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                    case "Student":
                    default:
                        return await _db.Database
                            .GetCollection<Student>("Users")
                            .FindAsync(x => x.Id == userId, cancellationToken: token);
                }
            }

            private async Task<string> GetNextRegistrationId(CancellationToken token)
            {
                var userCount = await _db.Database
                    .GetCollection<User>("Users")
                    .EstimatedDocumentCountAsync(cancellationToken: token);

                string prefix = DateTimeOffset.Now.Year.ToString();
                return prefix + (userCount + 1);
            }
        }
    }
}
