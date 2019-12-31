using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Events;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.Users;
using Domain.Base;
using Domain.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Functional;

namespace Domain.ECommerceIntegration.Commands
{
    public class MigrateStructure
    {
        public class Contract : CommandContract<Result>
        {
            public string UserRole { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private readonly IConfiguration _configuration;
            private readonly IEmailProvider _provider;

            public Handler(
                IDbContext db,
                UserManager<User> userManager,
                IMediator mediator,
                IEmailProvider provider,
                IConfiguration configuration
            ) {
                _db = db;
                _userManager = userManager;
                _mediator = mediator;
                _provider = provider;
                _configuration = configuration;
            }

            public async Task<Result> Handle(Contract request, CancellationToken cancellationToken)
            {
                string config = _configuration[$"EcommerceIntegration:Active"];

                if (config == null || config != "True")
                    return Result.Fail("Acesso Negado");

                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail("Acesso Negado");

                var tracks = await GetTracksWithEcommerceId(cancellationToken);

                foreach (var track in tracks)
                {
                    track.EcommerceProducts = new List<EcommerceProduct> {
                        EcommerceProduct.Create(
                            track.EcommerceId.Value, 1, false, null, false, null, null, null, null
                        ).Data
                    };

                    await _db.TrackCollection.ReplaceOneAsync(t =>
                        t.Id == track.Id, track,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok();
            }

            private async Task<List<Track>> GetTracksWithEcommerceId(CancellationToken token)
            {
                return await _db.TrackCollection.AsQueryable()
                    .Where(mod => mod.EcommerceId.HasValue && mod.EcommerceId.Value > 0)
                    .ToListAsync(cancellationToken: token);
            }
        }
    }
}
