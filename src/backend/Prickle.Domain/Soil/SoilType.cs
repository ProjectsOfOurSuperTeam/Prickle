namespace Prickle.Domain.Soil;

public sealed class SoilType : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? HexColor { get; private set; }

    private SoilType()
    {
    }
    private SoilType(string name, string? hexColor) : this()
    {
        Name = name;
        HexColor = hexColor;
    }

    public static Result<SoilType> Create(string name, string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<SoilType>(SoilErrors.SoilType.EmptyName);
        }

        var soilType = new SoilType(name.Trim(), hexColor?.Trim());
        return Result.Success(soilType);
    }

    public Result<SoilType> Update(string newName, string? newHexColor)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure<SoilType>(SoilErrors.SoilType.EmptyName);
        }

        Name = newName.Trim();
        HexColor = newHexColor?.Trim();
        return Result.Success(this);
    }
}
