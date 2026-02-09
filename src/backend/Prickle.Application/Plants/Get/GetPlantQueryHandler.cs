using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.Get;

internal sealed class GetPlantQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetPlantQuery, Result<PlantResponse>>
{
    public async ValueTask<Result<PlantResponse>> Handle(GetPlantQuery query, CancellationToken cancellationToken)
    {
        var plant = await dbContext.Plants
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

        if (plant is null)
        {
            return Result.Failure<PlantResponse>(PlantErrors.NotFound(query.Id));
        }

        var response = new PlantResponse
        {
            Id = plant.Id,
            Name = plant.Name,
            NameLatin = plant.NameLatin,
            Description = plant.Description,
            ImageUrl = plant.ImageUrl,
            ImageIsometricUrl = plant.ImageIsometricUrl,
            LightLevel = plant.LightLevel,
            WaterNeed = plant.WaterNeed,
            HumidityLevel = plant.HumidityLevel,
            ItemMaxSize = plant.ItemMaxSize,
            SoilFormulaId = plant.SoilFormulaId
        };

        return Result.Success(response);
    }
}