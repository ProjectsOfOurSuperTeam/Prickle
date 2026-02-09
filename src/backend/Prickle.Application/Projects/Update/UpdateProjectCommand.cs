namespace Prickle.Application.Projects.Update;

public sealed record UpdateProjectCommand(
    Guid Id,
    Guid UserId,
    byte[]? Preview) : ICommand<Result<ProjectResponse>>;