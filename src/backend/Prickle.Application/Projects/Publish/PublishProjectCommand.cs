namespace Prickle.Application.Projects.Publish;

public sealed record PublishProjectCommand(Guid Id, Guid UserId) : ICommand<Result<ProjectResponse>>;