namespace Prickle.Domain.Projects;

public class Project : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ContainerId { get; private set; }
    public byte[]? Preview { get; private set; }
    public byte[]? GeneratedFlorariumImage { get; private set; }
    public string? GeneratedFlorariumImageMimeType { get; private set; }
    public FlorariumImageGenerationStatus FlorariumImageGenerationStatus { get; private set; }
    public byte[]? PendingFlorariumCanvasImage { get; private set; }
    public string? PendingFlorariumCanvasImageMimeType { get; private set; }
    public DateTimeOffset? FlorariumImageRequestedAt { get; private set; }
    public DateTimeOffset? FlorariumImageCompletedAt { get; private set; }
    public string? FlorariumImageGenerationError { get; private set; }
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
        byte[]? generatedFlorariumImage,
        string? generatedFlorariumImageMimeType,
        FlorariumImageGenerationStatus florariumImageGenerationStatus,
        byte[]? pendingFlorariumCanvasImage,
        string? pendingFlorariumCanvasImageMimeType,
        DateTimeOffset? florariumImageRequestedAt,
        DateTimeOffset? florariumImageCompletedAt,
        string? florariumImageGenerationError,
        DateTimeOffset createdAt,
        bool isPublished)
    {
        Id = id;
        UserId = userId;
        ContainerId = containerId;
        Preview = preview;
        GeneratedFlorariumImage = generatedFlorariumImage;
        GeneratedFlorariumImageMimeType = generatedFlorariumImageMimeType;
        FlorariumImageGenerationStatus = florariumImageGenerationStatus;
        PendingFlorariumCanvasImage = pendingFlorariumCanvasImage;
        PendingFlorariumCanvasImageMimeType = pendingFlorariumCanvasImageMimeType;
        FlorariumImageRequestedAt = florariumImageRequestedAt;
        FlorariumImageCompletedAt = florariumImageCompletedAt;
        FlorariumImageGenerationError = florariumImageGenerationError;
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
            null,
            null,
            FlorariumImageGenerationStatus.NotRequested,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            false);

        return Result.Success(project);
    }

    public Result<Project> UpdatePreview(byte[]? preview)
    {
        Preview = preview;
        return Result.Success(this);
    }

    public Result<Project> QueueFlorariumImageGeneration(byte[] canvasImage, string canvasImageMimeType)
    {
        if (canvasImage.Length == 0)
        {
            return Result.Failure<Project>(Error.Problem(
                "Project.EmptyCanvasImage",
                "Canvas image is required to start florarium generation."));
        }

        if (string.IsNullOrWhiteSpace(canvasImageMimeType))
        {
            return Result.Failure<Project>(Error.Problem(
                "Project.EmptyCanvasImageMimeType",
                "Canvas image mime type is required to start florarium generation."));
        }

        if (FlorariumImageGenerationStatus == FlorariumImageGenerationStatus.Pending)
        {
            return Result.Failure<Project>(Error.Conflict(
                "Project.FlorariumImageGenerationInProgress",
                "Florarium image generation is already in progress for this project."));
        }

        PendingFlorariumCanvasImage = canvasImage;
        PendingFlorariumCanvasImageMimeType = canvasImageMimeType;
        FlorariumImageGenerationStatus = FlorariumImageGenerationStatus.Pending;
        FlorariumImageRequestedAt = DateTimeOffset.UtcNow;
        FlorariumImageCompletedAt = null;
        FlorariumImageGenerationError = null;

        return Result.Success(this);
    }

    public Result<Project> CompleteFlorariumImageGeneration(byte[] generatedImage, string generatedImageMimeType)
    {
        if (generatedImage.Length == 0)
        {
            return Result.Failure<Project>(Error.Problem(
                "Project.EmptyGeneratedFlorariumImage",
                "Generated florarium image cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(generatedImageMimeType))
        {
            return Result.Failure<Project>(Error.Problem(
                "Project.EmptyGeneratedFlorariumImageMimeType",
                "Generated florarium image mime type is required."));
        }

        GeneratedFlorariumImage = generatedImage;
        GeneratedFlorariumImageMimeType = generatedImageMimeType;
        Preview = generatedImage;
        PendingFlorariumCanvasImage = null;
        PendingFlorariumCanvasImageMimeType = null;
        FlorariumImageGenerationStatus = FlorariumImageGenerationStatus.Succeeded;
        FlorariumImageCompletedAt = DateTimeOffset.UtcNow;
        FlorariumImageGenerationError = null;

        return Result.Success(this);
    }

    public Result<Project> FailFlorariumImageGeneration(string errorDescription)
    {
        FlorariumImageGenerationStatus = FlorariumImageGenerationStatus.Failed;
        FlorariumImageCompletedAt = DateTimeOffset.UtcNow;
        FlorariumImageGenerationError = string.IsNullOrWhiteSpace(errorDescription)
            ? "Florarium image generation failed."
            : errorDescription;
        PendingFlorariumCanvasImage = null;
        PendingFlorariumCanvasImageMimeType = null;

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