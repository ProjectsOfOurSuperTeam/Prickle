namespace Prickle.Application.Containers.Get;

public sealed record GetContainerQuery(Guid Id) : IQuery<Result<ContainerResponse>>;