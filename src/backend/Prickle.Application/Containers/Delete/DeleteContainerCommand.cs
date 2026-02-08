namespace Prickle.Application.Containers.Delete;

public sealed record DeleteContainerCommand(Guid Id) : ICommand<Result>;