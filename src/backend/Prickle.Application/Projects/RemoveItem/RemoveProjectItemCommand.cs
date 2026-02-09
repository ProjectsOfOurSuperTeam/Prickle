namespace Prickle.Application.Projects.RemoveItem;

public sealed record RemoveProjectItemCommand(
    Guid ProjectId,
    Guid UserId,
    Guid ItemId) : ICommand<Result>;