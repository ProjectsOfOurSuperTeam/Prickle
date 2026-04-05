namespace Prickle.Application.Projects;

public sealed record ProjectResponse
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid ContainerId { get; init; }
    public byte[]? Preview { get; init; }
    public byte[]? GeneratedFlorariumImage { get; init; }
    public string? GeneratedFlorariumImageMimeType { get; init; }
    public required string FlorariumImageGenerationStatus { get; init; }
    public string? FlorariumImageGenerationError { get; init; }
    public DateTimeOffset? FlorariumImageRequestedAt { get; init; }
    public DateTimeOffset? FlorariumImageCompletedAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required bool IsPublished { get; init; }
    public IReadOnlyList<ProjectItemResponse> Items { get; init; } = [];
}