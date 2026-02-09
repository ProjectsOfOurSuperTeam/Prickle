namespace Prickle.Domain.Projects;

public class Project : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ContainerId { get; private set; }
    public byte[]? Preview { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsPublished { get; private set; }

    private readonly List<ProjectItem> _items = [];
    public IReadOnlyCollection<ProjectItem> Items => _items.AsReadOnly();

    private Project() { }

    private Project(
        Guid id,
        Guid userId,
        Guid containerId,
        byte[]? preview,
        DateTimeOffset createdAt,
        bool isPublished)
    {
        Id = id;
        UserId = userId;
        ContainerId = containerId;
        Preview = preview;
        CreatedAt = createdAt;
        IsPublished = isPublished;
    }

    public static Result<Project> Create(
        Guid userId,
        Guid containerId,
        byte[]? preview = null)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Project>(ProjectErrors.EmptyUserId);
        }

        if (containerId == Guid.Empty)
        {
            return Result.Failure<Project>(ProjectErrors.EmptyContainerId);
        }

        var project = new Project(
            Guid.NewGuid(),
            userId,
            containerId,
            preview,
            DateTimeOffset.UtcNow,
            false);

        return Result.Success(project);
    }

    public Result<Project> UpdatePreview(byte[]? preview)
    {
        Preview = preview;
        return Result.Success(this);
    }

    public Result<Project> Publish()
    {
        IsPublished = true;
        return Result.Success(this);
    }

    public Result<Project> Unpublish()
    {
        IsPublished = false;
        return Result.Success(this);
    }

    public Result<ProjectItem> AddItem(
        ProjectItemType itemType,
        Guid itemId,
        int posX,
        int posY,
        int posZ)
    {
        var itemResult = ProjectItem.Create(Id, itemType, itemId, posX, posY, posZ);
        if (itemResult.IsFailure)
        {
            return Result.Failure<ProjectItem>(itemResult.Error);
        }

        _items.Add(itemResult.Value);
        return Result.Success(itemResult.Value);
    }

    public Result RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return Result.Failure(ProjectErrors.ProjectItemNotFound(itemId));
        }

        _items.Remove(item);
        return Result.Success();
    }
}