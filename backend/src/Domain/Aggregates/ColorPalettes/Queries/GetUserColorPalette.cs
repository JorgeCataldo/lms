using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.ColorPalettes;
using Domain.Data;
using Domain.Base;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.ColorPalletes.Queries
{
    public class GetUserColorPalette
    {
        public class Contract : CommandContract<Result<ColorPalette>>
        {
            public string CurrentUserRole { get; set; }
            public string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<ColorPalette>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<ColorPalette>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail<ColorPalette>("Acesso Negado");

                if (String.IsNullOrEmpty(request.CurrentUserRole) || request.CurrentUserRole != "BusinessManager")
                    return Result.Fail<ColorPalette>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var colorPallete = await _db.ColorPaletteCollection
                    .AsQueryable()
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (colorPallete == null)
                    return Result.Ok(new ColorPalette()
                    {
                        UserId = userId,
                        ColorBaseValues = new List<ColorBaseValue>()
                    });

                return Result.Ok(colorPallete);
            }
        }
    }
}
