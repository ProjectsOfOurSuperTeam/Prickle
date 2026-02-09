using Ardalis.SmartEnum;

namespace Prickle.Domain.Plants;

public sealed class PlantHumidityLevel : SmartEnum<PlantHumidityLevel, int>
{
    public static readonly PlantHumidityLevel VeryLow = new(nameof(VeryLow), 1);
    public static readonly PlantHumidityLevel Low = new(nameof(Low), 2);
    public static readonly PlantHumidityLevel Medium = new(nameof(Medium), 3);
    public static readonly PlantHumidityLevel High = new(nameof(High), 4);
    public static readonly PlantHumidityLevel VeryHigh = new(nameof(VeryHigh), 5);

    private PlantHumidityLevel(string name, int value)
        : base(name, value)
    {
    }
}
