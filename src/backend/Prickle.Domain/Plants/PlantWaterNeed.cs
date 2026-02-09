using Ardalis.SmartEnum;

namespace Prickle.Domain.Plants;

public sealed class PlantWaterNeed : SmartEnum<PlantWaterNeed, int>
{
    public static readonly PlantWaterNeed VeryLow = new(nameof(VeryLow), 1);
    public static readonly PlantWaterNeed Low = new(nameof(Low), 2);
    public static readonly PlantWaterNeed Medium = new(nameof(Medium), 3);
    public static readonly PlantWaterNeed High = new(nameof(High), 4);
    public static readonly PlantWaterNeed VeryHigh = new(nameof(VeryHigh), 5);

    private PlantWaterNeed(string name, int value)
        : base(name, value)
    {
    }
}
