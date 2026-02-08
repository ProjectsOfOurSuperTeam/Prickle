namespace Prickle.Application.Containers.Add;

public sealed record AddContainerCommand(
    string Name,
    string? Description,
    float Volume,
    bool IsClosed,
    string? ImageUrl,
    string? ImageIsometricUrl) : ICommand<Result<ContainerResponse>>;