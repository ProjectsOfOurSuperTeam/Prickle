namespace Prickle.Application.Abstractions.ImageGeneration;

public interface IFlorariumImageGenerator
{
    Task<Result<byte[]>> GenerateFlorariumImageAsync(
        string prompt,
        byte[] atlasImage,
        byte[] layoutImage,
        string imageMimeType,
        CancellationToken cancellationToken = default);
}
