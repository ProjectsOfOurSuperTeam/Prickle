namespace Prickle.Application.Projects.GenerateFlorariumImage;

public sealed record GenerateFlorariumImageResponse(byte[] ImageBytes, string MimeType = "image/png");
