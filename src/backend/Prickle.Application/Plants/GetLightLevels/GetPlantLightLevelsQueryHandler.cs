using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.GetLightLevels;

internal class GetPlantLightLevelsQueryHandler
    : IQueryHandler<GetPlantLightLevelsQuery, Result<PlantLightLevelsResponse>>
{
    public ValueTask<Result<PlantLightLevelsResponse>> Handle(GetPlantLightLevelsQuery query, CancellationToken cancellationToken)
    {
        var lightLevels = PlantLightLevel.List
            .Select(l => new PlantLightLevelResponse(l.Value, l.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new PlantLightLevelsResponse(lightLevels)));
    }
}