using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.GetWaterNeeds;

internal class GetPlantWaterNeedsQueryHandler
    : IQueryHandler<GetPlantWaterNeedsQuery, Result<PlantWaterNeedsResponse>>
{
    public ValueTask<Result<PlantWaterNeedsResponse>> Handle(GetPlantWaterNeedsQuery query, CancellationToken cancellationToken)
    {
        var waterNeeds = PlantWaterNeed.List
            .Select(w => new PlantWaterNeedResponse(w.Value, w.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new PlantWaterNeedsResponse(waterNeeds)));
    }
}