namespace Prickle.Application.Plants.GetPlantCategories;

public record PlantCategoriesResponse(IReadOnlyList<PlantCategoryResponse> Categories);