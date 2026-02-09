namespace Prickle.Application.Projects.Delete;

public sealed record DeleteProjectCommand(Guid Id, Guid UserId) : ICommand<Result>;