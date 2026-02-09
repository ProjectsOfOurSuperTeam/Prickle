namespace Prickle.Application.Projects;

public sealed record ProjectResponse
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid ContainerId { get; init; }
    public byte[]? Preview { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required bool IsPublished { get; init; }
    public IReadOnlyList<ProjectItemResponse> Items { get; init; } = [];
}