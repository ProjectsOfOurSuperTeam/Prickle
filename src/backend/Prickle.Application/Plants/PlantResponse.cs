using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;

namespace Prickle.Application.Plants;

public sealed record PlantResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string NameLatin { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageIsometricUrl { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<PlantLightLevel, int>))]
    public required PlantLightLevel LightLevel { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<PlantWaterNeed, int>))]
    public required PlantWaterNeed WaterNeed { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<PlantHumidityLevel, int>))]
    public required PlantHumidityLevel HumidityLevel { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<ProjectItemSize, int>))]
    public required ProjectItemSize ItemMaxSize { get; init; }
    public required Guid SoilFormulaId { get; init; }
}