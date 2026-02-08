namespace Prickle.Application.Containers;

public sealed record ContainerResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required float Volume { get; init; }
    public required bool IsClosed { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageIsometricUrl { get; init; }
}