using Ardalis.SmartEnum;

namespace Prickle.Domain.Plants;

public sealed class PlantLightLevel : SmartEnum<PlantLightLevel, int>
{
    public static readonly PlantLightLevel VeryLow = new(nameof(VeryLow), 1);
    public static readonly PlantLightLevel Low = new(nameof(Low), 2);
    public static readonly PlantLightLevel Medium = new(nameof(Medium), 3);
    public static readonly PlantLightLevel High = new(nameof(High), 4);
    public static readonly PlantLightLevel VeryHigh = new(nameof(VeryHigh), 5);

    private PlantLightLevel(string name, int value)
        : base(name, value)
    {
    }
}
