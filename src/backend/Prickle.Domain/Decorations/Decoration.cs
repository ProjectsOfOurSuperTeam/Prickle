using Prickle.Domain.Projects;

namespace Prickle.Domain.Decorations;

public sealed class Decoration : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; } = default!;
    public DecorationCategory Category { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImageIsometricUrl { get; private set; }
    public ProjectItemSize ItemMaxSize { get; private set; } = default!;

    private Decoration() { }

    private Decoration(
        Guid id,
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl,
        ProjectItemSize itemMaxSize)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        ImageUrl = imageUrl;
        ImageIsometricUrl = imageIsometricUrl;
        ItemMaxSize = itemMaxSize;
    }

    public static Result<Decoration> Create(
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl,
        ProjectItemSize itemMaxSize)
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
            imageIsometricUrl,
            itemMaxSize);

        return Result.Success(decoration);
    }

    public Result<Decoration> Update(
        string name,
        string? description,
        DecorationCategory category,
        string? imageUrl,
        string? imageIsometricUrl,
        ProjectItemSize itemMaxSize)
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
        ItemMaxSize = itemMaxSize;
        return Result.Success(this);
    }
}
