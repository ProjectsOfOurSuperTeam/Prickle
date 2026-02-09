namespace Prickle.Application.Plants.GetWaterNeeds;

public sealed record PlantWaterNeedsResponse(IReadOnlyList<PlantWaterNeedResponse> WaterNeeds);