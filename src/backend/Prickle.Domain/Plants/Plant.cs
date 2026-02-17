using Prickle.Domain.Projects;

namespace Prickle.Domain.Plants;

public class Plant : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string NameLatin { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImageIsometricUrl { get; private set; }
    public PlantCategory Category { get; private set; } = default!;
    public PlantLightLevel LightLevel { get; private set; } = default!;
    public PlantWaterNeed WaterNeed { get; private set; } = default!;
    public PlantHumidityLevel HumidityLevel { get; private set; } = default!;
    public ProjectItemSize ItemMaxSize { get; private set; } = default!;
    public Guid SoilFormulaId { get; private set; }

    private Plant() { }

    protected Plant(Guid id,
        string name,
        string nameLatin,
        string? description,
        string? imageUrl,
        string? imageIsometricUrl,
        PlantCategory category,
        PlantLightLevel lightLevel,
        PlantWaterNeed waterNeed,
        PlantHumidityLevel humidityLevel,
        ProjectItemSize itemMaxSize,
        Guid soilFormulaId)
    {
        Id = id;
        Name = name;
        NameLatin = nameLatin;
        Description = description;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
        Category = category;
        LightLevel = lightLevel;
        WaterNeed = waterNeed;
        HumidityLevel = humidityLevel;
        ItemMaxSize = itemMaxSize;
        SoilFormulaId = soilFormulaId;
    }

    public static Result<Plant> Create(
        string name,
        string nameLatin,
        string? description,
        string? imageUrl,
        string? imageIsometricUrl,
        PlantCategory category,
        PlantLightLevel lightLevel,
        PlantWaterNeed waterNeed,
        PlantHumidityLevel humidityLevel,
        ProjectItemSize itemMaxSize,
        Guid soilFormulaId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Plant>(PlantErrors.EmptyName);
        }

        if (string.IsNullOrWhiteSpace(nameLatin))
        {
            return Result.Failure<Plant>(PlantErrors.EmptyNameLatin);
        }

        var plant = new Plant(
            Guid.NewGuid(),
            name.Trim(),
            nameLatin.Trim(),
            description?.Trim(),
            imageUrl,
            imageIsometricUrl,
            category,
            lightLevel,
            waterNeed,
            humidityLevel,
            itemMaxSize,
            soilFormulaId);

        return Result.Success(plant);
    }

    public Result<Plant> Update(
        string name,
        string nameLatin,
        string? description,
        string? imageUrl,
        string? imageIsometricUrl,
        PlantCategory category,
        PlantLightLevel lightLevel,
        PlantWaterNeed waterNeed,
        PlantHumidityLevel humidityLevel,
        ProjectItemSize itemMaxSize,
        Guid soilFormulaId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Plant>(PlantErrors.EmptyName);
        }

        if (string.IsNullOrWhiteSpace(nameLatin))
        {
            return Result.Failure<Plant>(PlantErrors.EmptyNameLatin);
        }

        Name = name.Trim();
        NameLatin = nameLatin.Trim();
        Description = description?.Trim();
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
        Category = category;
        LightLevel = lightLevel;
        WaterNeed = waterNeed;
        HumidityLevel = humidityLevel;
        ItemMaxSize = itemMaxSize;
        SoilFormulaId = soilFormulaId;
        return Result.Success(this);
    }
}
