namespace Prickle.Application.Projects.GenerateFlorariumImage;

public sealed record GenerateFlorariumImageCommand(
    Guid ProjectId,
    Guid UserId,
    byte[] AtlasImage,
    byte[] LayoutImage,
    string ImageMimeType = "image/png") : ICommand<Result<GenerateFlorariumImageResponse>>;
