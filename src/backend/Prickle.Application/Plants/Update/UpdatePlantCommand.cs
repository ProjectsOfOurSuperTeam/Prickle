using Prickle.Domain.Plants;
using Prickle.Domain.Projects;

namespace Prickle.Application.Plants.Update;

public sealed record UpdatePlantCommand(
    Guid Id,
    string Name,
    string NameLatin,
    string? Description,
    string? ImageUrl,
    string? ImageIsometricUrl,
    PlantCategory Category,
    PlantLightLevel LightLevel,
    PlantWaterNeed WaterNeed,
    PlantHumidityLevel HumidityLevel,
    ProjectItemSize ItemMaxSize,
    Guid SoilFormulaId) : ICommand<Result<PlantResponse>>;