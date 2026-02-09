namespace Prickle.Application.Projects.UpdateItem;

public sealed record UpdateProjectItemCommand(
    Guid ProjectId,
    Guid UserId,
    Guid ItemId,
    int PosX,
    int PosY,
    int PosZ) : ICommand<Result<ProjectItemResponse>>;