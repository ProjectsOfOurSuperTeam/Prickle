namespace Prickle.Domain.Plants;

public static class PlantErrors
{
    public static readonly Error EmptyName = Error.Problem(
    "Plant.EmptyName",
    "Plant name cannot be empty");

    public static readonly Error EmptyNameLatin = Error.Problem(
    "Plant.EmptyNameLatin",
    "Plant latin name cannot be empty");

    public static Error NotFound(Guid id) => Error.Problem(
        "Plant.NotFound",
        $"Plant with ID '{id}' was not found"
    );

    public static Error AlreadyExists(string name) => Error.Problem(
        "Plant.AlreadyExists",
        $"Plant with name '{name}' already exists"
    );
    public static Error InvalidCategory(int category) => Error.Problem(
        "Plant.InvalidCategory",
        $"Invalid category '{category}'"
    );

    public static Error InvalidLightLevel(int lightLevel) => Error.Problem(
        "Plant.InvalidLightLevel",
        $"Invalid light level '{lightLevel}'"
    );

    public static Error InvalidWaterNeed(int waterNeed) => Error.Problem(
        "Plant.InvalidWaterNeed",
        $"Invalid water need '{waterNeed}'"
    );

    public static Error InvalidHumidityLevel(int humidityLevel) => Error.Problem(
        "Plant.InvalidHumidityLevel",
        $"Invalid humidity level '{humidityLevel}'"
    );

    public static Error InvalidItemSize(int itemSize) => Error.Problem(
        "Plant.InvalidItemSize",
        $"Invalid item size '{itemSize}'"
    );
}
