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

namespace Domain.Aggregates.ColorPalletes.Commands
{
    public class UpdateUserColorPalette
    {
        public class Contract : CommandContract<Result<Contract>>
        {
            public string CurrentUserRole { get; set; }
            public string UserId { get; set; }
            public List<ColorBaseValue> ColorBaseValues { get; set; }
        }

        public class Handler : IRequestHandler<Contract, Result<Contract>>
        {
            private readonly IDbContext _db;

            public Handler(IDbContext db)
            {
                _db = db;
            }

            public async Task<Result<Contract>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.UserId))
                    return Result.Fail<Contract>("Acesso Negado");

                if (String.IsNullOrEmpty(request.CurrentUserRole) || request.CurrentUserRole != "BusinessManager")
                    return Result.Fail<Contract>("Acesso Negado");

                var userId = ObjectId.Parse(request.UserId);

                var colorPallete = await _db.ColorPaletteCollection
                    .AsQueryable()
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (colorPallete == null)
                {
                    await _db.ColorPaletteCollection.InsertOneAsync(
                        new ColorPalette() { UserId = userId, ColorBaseValues = request.ColorBaseValues },
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    colorPallete.ColorBaseValues = request.ColorBaseValues;

                    await _db.ColorPaletteCollection.ReplaceOneAsync(x =>
                        x.UserId == userId, colorPallete,
                        cancellationToken: cancellationToken
                    );
                }

                return Result.Ok(request);
            }
        }
    }
}
