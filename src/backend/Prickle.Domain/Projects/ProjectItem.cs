namespace Prickle.Domain.Projects;

public class ProjectItem : Entity
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public ProjectItemType ItemType { get; private set; } = default!;
    public Guid ItemId { get; private set; }
    public int PosX { get; private set; }
    public int PosY { get; private set; }
    public int PosZ { get; private set; }

    private ProjectItem() { }

    private ProjectItem(
        Guid id,
        Guid projectId,
        ProjectItemType itemType,
        Guid itemId,
        int posX,
        int posY,
        int posZ)
    {
        Id = id;
        ProjectId = projectId;
        ItemType = itemType;
        ItemId = itemId;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }

    public static Result<ProjectItem> Create(
        Guid projectId,
        ProjectItemType itemType,
        Guid itemId,
        int posX,
        int posY,
        int posZ)
    {
        if (projectId == Guid.Empty)
        {
            return Result.Failure<ProjectItem>(ProjectErrors.EmptyProjectId);
        }

        if (itemId == Guid.Empty)
        {
            return Result.Failure<ProjectItem>(ProjectErrors.EmptyItemId);
        }

        var projectItem = new ProjectItem(
            Guid.NewGuid(),
            projectId,
            itemType,
            itemId,
            posX,
            posY,
            posZ);

        return Result.Success(projectItem);
    }

    public Result<ProjectItem> UpdatePosition(int posX, int posY, int posZ)
    {
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        return Result.Success(this);
    }
}