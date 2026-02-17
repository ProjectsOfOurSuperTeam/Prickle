using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.GetPlantCategories;

internal sealed class GetPlantCategoriesQueryHandler : IQueryHandler<GetPlantCategoriesQuery, Result<PlantCategoriesResponse>>
{
    public ValueTask<Result<PlantCategoriesResponse>> Handle(GetPlantCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = PlantCategory.List
            .Select(c => new PlantCategoryResponse(c.Value, c.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new PlantCategoriesResponse(categories)));
    }
}
