namespace Prickle.Application.Projects.Add;

public sealed record AddProjectCommand(
    Guid UserId,
    Guid ContainerId,
    byte[]? Preview = null) : ICommand<Result<ProjectResponse>>;