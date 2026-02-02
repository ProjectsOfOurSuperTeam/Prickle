namespace Prickle.Domain.Soil;

public sealed class SoilType : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;

    private SoilType()
    {
    }
    private SoilType(string name) : this()
    {
        Name = name;
    }

    public static Result<SoilType> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<SoilType>(SoilErrors.SoilType.EmptyName);
        }

        var soilType = new SoilType(name.Trim());
        return Result.Success(soilType);
    }

    public Result<SoilType> Update(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure<SoilType>(SoilErrors.SoilType.EmptyName);
        }

        Name = newName.Trim();
        return Result.Success(this);
    }
}
