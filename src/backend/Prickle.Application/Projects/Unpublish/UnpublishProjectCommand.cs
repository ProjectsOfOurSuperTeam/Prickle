namespace Prickle.Application.Projects.Unpublish;

public sealed record UnpublishProjectCommand(Guid Id, Guid UserId) : ICommand<Result<ProjectResponse>>;