using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.GetHumidityLevels;

internal class GetPlantHumidityLevelsQueryHandler
    : IQueryHandler<GetPlantHumidityLevelsQuery, Result<PlantHumidityLevelsResponse>>
{
    public ValueTask<Result<PlantHumidityLevelsResponse>> Handle(GetPlantHumidityLevelsQuery query, CancellationToken cancellationToken)
    {
        var humidityLevels = PlantHumidityLevel.List
            .Select(h => new PlantHumidityLevelResponse(h.Value, h.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new PlantHumidityLevelsResponse(humidityLevels)));
    }
}