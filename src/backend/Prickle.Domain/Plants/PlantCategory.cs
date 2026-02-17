using Ardalis.SmartEnum;

namespace Prickle.Domain.Plants;

public sealed class PlantCategory : SmartEnum<PlantCategory, int>
{
    public static readonly PlantCategory NoCategory = new(nameof(NoCategory), 0);
    public static readonly PlantCategory Succulents = new(nameof(Succulents), 1); // Сукуленти
    public static readonly PlantCategory Cacti = new(nameof(Cacti), 2); // Кактуси
    public static readonly PlantCategory Tropical = new(nameof(Tropical), 3); // Тропічні
    public static readonly PlantCategory Ferns = new(nameof(Ferns), 4); // Папороті
    public static readonly PlantCategory Mosses = new(nameof(Mosses), 5); // Мохи
    public static readonly PlantCategory CarnivorousPlants = new(nameof(CarnivorousPlants), 6); // Хижі рослини

    private PlantCategory(string name, int value)
        : base(name, value)
    {
    }
}