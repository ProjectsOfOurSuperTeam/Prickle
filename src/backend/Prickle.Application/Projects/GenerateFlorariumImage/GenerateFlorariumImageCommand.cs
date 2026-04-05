namespace Prickle.Application.Projects.GenerateFlorariumImage;

public sealed record GenerateFlorariumImageCommand(
    Guid ProjectId,
    Guid UserId,
    byte[] CanvasImage,
    string ImageMimeType = "image/png") : ICommand<Result<ProjectResponse>>;
