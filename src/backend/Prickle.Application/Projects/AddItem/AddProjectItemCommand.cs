using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.AddItem;

public sealed record AddProjectItemCommand(
    Guid ProjectId,
    Guid UserId,
    ProjectItemType ItemType,
    Guid ItemId,
    int PosX,
    int PosY,
    int PosZ) : ICommand<Result<ProjectItemResponse>>;