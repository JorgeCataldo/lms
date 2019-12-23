using Domain.Aggregates.Events;
using Domain.Data;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tg4.Infrastructure.Functional;
using static Domain.Aggregates.Users.User;

namespace Domain.Aggregates.Users.Queries
{
    public class GetUserBasicProgressInfoQuery
    {
        public class Contract : CommandContract<Result<UserBasicProgressItem>>
        {
            public string UserId { get; set; }
            public string EntityId { get; set; }
        }

        public class UserBasicProgressItem
        {
            
            public List<UserProgressItem> ModulesInfo { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<UserProgressItem> EventsInfo { get; set; }            
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string UserName { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Phone2 { get; set; }
            public bool IsBlocked { get; set; }
            public bool IsEmailConfirmed { get; set; }
            public string Cpf { get; set; }
            public Address Address { get; set; }
            public string ImageUrl { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public string ResponsibleName { get; set; }
            public string RegistrationId { get; set; }
            public string Info { get; set; }
            public List<UserProgressItem> ModulesInfo { get; set; }
            public List<UserProgressItem> TracksInfo { get; set; }
            public List<UserProgressItem> EventsInfo { get; set; }
            public List<string> AcquiredKnowledge { get; set; }
            public string LineManager { get; set; }
            public string LineManagerEmail { get; set; }
            public DateTimeOffset ServiceDate { get; set; }
            public string Education { get; set; }
            public string Gender { get; set; }
            public bool? Manager { get; set; }
            public long? Cge { get; set; }
            public long? IdCr { get; set; }
            public string CoHead { get; set; }
            public RelationalItem Company { get; set; }
            public RelationalItem BusinessGroup { get; set; }
            public RelationalItem BusinessUnit { get; set; }
            public RelationalItem Country { get; set; }
            public string Language { get; set; }
            public RelationalItem FrontBackOffice { get; set; }
            public RelationalItem Job { get; set; }
            public RelationalItem Location { get; set; }
            public RelationalItem Rank { get; set; }
            public List<RelationalItem> Sectors { get; set; }
            public RelationalItem Segment { get; set; }
            public string Role { get; set; }
            public DocumentType? Document { get; set; }
            public string DocumentNumber { get; set; }
            public string DocumentEmitter { get; set; }
            public DateTimeOffset? EmitDate { get; set; }
            public DateTimeOffset? ExpirationDate { get; set; }
            public bool? SpecialNeeds { get; set; }
            public string SpecialNeedsDescription { get; set; }
            public string LinkedIn { get; set; }
            public DateTimeOffset? DateBorn { get; set; }
            public List<string> BlockedFields { get; set; }
        }

        public class UserProgressItem
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public bool Blocked { get; set; }
            public int? ValidFor { get; set; }
            public DateTimeOffset? DueDate { get; set; }
        }
        
        public class Handler : IRequestHandler<Contract, Result<UserBasicProgressItem>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(
                IDbContext db, 
                IMediator mediator,
                IConfiguration configuration
            ) {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<UserBasicProgressItem>> Handle(Contract request, CancellationToken cancellationToken)
            {
                try
                {
                    var userId = ObjectId.Parse(request.UserId);
                    var entityId = ObjectId.Parse(request.EntityId);

                    var qry = await _db
                        .Database
                        .GetCollection<UserItem>("Users")
                        .FindAsync(x => x.Id == userId, cancellationToken: cancellationToken);

                    var user = await qry.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (user == null)
                        return Result.Fail<UserBasicProgressItem>("Usuário não existe");

                    user.EventsInfo = user.EventsInfo ?? new List<UserProgressItem>();
                    foreach (var req in user.EventsInfo)
                    {
                        var eve = await (await _db
                                .Database
                                .GetCollection<UserProgressItem>("Events")
                                .FindAsync(x => x.Id == req.Id, cancellationToken: cancellationToken))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (eve != null)
                        {
                            req.Title = eve.Title;
                            req.ImageUrl = eve.ImageUrl;
                        }
                    }

                    user.ModulesInfo = user.ModulesInfo ?? new List<UserProgressItem>();
                    foreach (var req in user.ModulesInfo)
                    {
                        var mod = await (await _db
                                .Database
                                .GetCollection<UserProgressItem>("Modules")
                                .FindAsync(x => x.Id == req.Id, cancellationToken: cancellationToken))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (mod != null)
                        {
                            req.Title = mod.Title;
                            req.ImageUrl = mod.ImageUrl;
                        }
                    }

                    user.TracksInfo = user.TracksInfo ?? new List<UserProgressItem>();
                    foreach (var req in user.TracksInfo)
                    {
                        var tra = await (await _db
                                .Database
                                .GetCollection<UserProgressItem>("Tracks")
                                .FindAsync(x => x.Id == req.Id, cancellationToken: cancellationToken))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (tra != null)
                        {
                            req.Title = tra.Title;
                            req.ImageUrl = tra.ImageUrl;
                        }
                    }

                    var basicProgressInfo = new UserBasicProgressItem
                    {
                        EventsInfo = user.EventsInfo.Where(x => x.Id == entityId).ToList(),
                        ModulesInfo = user.ModulesInfo.Where(x => x.Id == entityId).ToList(),
                        TracksInfo = user.TracksInfo.Where(x => x.Id == entityId).ToList()
                    };

                    return Result.Ok(basicProgressInfo);
                }
                catch(Exception ex)
                {
                    var teste = ex;
                    return Result.Fail<UserBasicProgressItem>("Ocorreu um erro");
                }
           }

        }
    }
}
