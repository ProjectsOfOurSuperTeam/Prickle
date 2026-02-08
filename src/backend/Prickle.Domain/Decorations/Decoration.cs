namespace Prickle.Domain.Decorations;

public sealed class Decoration : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; } = default!;
    public DecorationCategory Category { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImageIsometricUrl { get; private set; }

    private Decoration() { }

    private Decoration(
        Guid id,
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
    }

    public static Result<Decoration> Create(
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Decoration>(DecorationErrors.EmptyName);
        }

        var decoration = new Decoration(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim() ?? string.Empty,
            category,
            imageUrl,
            imageIsometricUrl);

        return Result.Success(decoration);
    }

    public Result<Decoration> Update(
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Decoration>(DecorationErrors.EmptyName);
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Category = category;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
        return Result.Success(this);
    }
}