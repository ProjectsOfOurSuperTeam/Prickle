namespace Prickle.Application.Plants.GetLightLevels;

public sealed record PlantLightLevelsResponse(IReadOnlyList<PlantLightLevelResponse> LightLevels);