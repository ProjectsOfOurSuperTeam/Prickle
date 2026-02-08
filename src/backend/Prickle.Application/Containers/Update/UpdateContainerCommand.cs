namespace Prickle.Application.Containers.Update;

public sealed record UpdateContainerCommand(
    Guid Id,
    string Name,
    string? Description,
    float Volume,
    bool IsClosed,
    string? ImageUrl,
    string? ImageIsometricUrl) : ICommand<Result<ContainerResponse>>;