namespace Prickle.Domain.Containers;

public sealed class Container
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; } = default!;
    public float Volume { get; private set; }
    public bool IsClosed { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImageIsometricUrl { get; private set; }

    private Container() { }

    private Container(
    Guid id,
    string name,
    string? description,
    float volume,
    bool isClosed,
    string? imageUrl,
    string? imageIsometricUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        Volume = volume;
        IsClosed = isClosed;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
    }

    public static Result<Container> Create(string name,
    string? description,
    float volume,
    bool isClosed,
    string? imageUrl,
    string? imageIsometricUrl)
    {

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Container>(ContainerErrors.EmptyName);
        }

        if (volume <= 0)
        {
            return Result.Failure<Container>(ContainerErrors.InvalidVolume);
        }

        var container = new Container(
            Guid.NewGuid(),
            name.Trim(),
            description,
            volume,
            isClosed,
            imageUrl,
            imageIsometricUrl);
        return Result.Success(container);

    }

    public Result<Container> Update(string name,
    string? description,
    float volume,
    bool isClosed,
    string? imageUrl,
    string? imageIsometricUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Container>(ContainerErrors.EmptyName);
        }

        if (volume <= 0)
        {
            return Result.Failure<Container>(ContainerErrors.InvalidVolume);
        }

        Name = name.Trim();
        Description = description;
        Volume = volume;
        IsClosed = isClosed;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
        return Result.Success(this);
    }
}
