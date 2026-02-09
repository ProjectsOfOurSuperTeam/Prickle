using Prickle.Domain.Plants;
using Prickle.Domain.Projects;

namespace Prickle.Application.Plants.Add;

public sealed record AddPlantCommand(
    string Name,
    string NameLatin,
    string? Description,
    string? ImageUrl,
    string? ImageIsometricUrl,
    PlantLightLevel LightLevel,
    PlantWaterNeed WaterNeed,
    PlantHumidityLevel HumidityLevel,
    ProjectItemSize ItemMaxSize,
    Guid SoilFormulaId) : ICommand<Result<PlantResponse>>;