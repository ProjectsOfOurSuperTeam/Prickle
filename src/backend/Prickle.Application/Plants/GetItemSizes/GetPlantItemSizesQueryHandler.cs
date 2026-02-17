using Prickle.Domain.Projects;

namespace Prickle.Application.Plants.GetItemSizes;

internal sealed class GetPlantItemSizesQueryHandler
    : IQueryHandler<GetPlantItemSizesQuery, Result<PlantItemSizesResponse>>
{
    public ValueTask<Result<PlantItemSizesResponse>> Handle(GetPlantItemSizesQuery query, CancellationToken cancellationToken)
    {
        var itemSizes = ProjectItemSize.List
            .Select(s => new PlantItemSizeResponse(s.Value, s.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new PlantItemSizesResponse(itemSizes)));
    }
}