namespace Prickle.Application.Abstractions.ImageGeneration;

public interface IFlorariumImageGenerator
{
    Task<Result<byte[]>> GenerateFlorariumImageAsync(
        string prompt,
    string containerImageReference,
    byte[] canvasImage,
        string imageMimeType,
        CancellationToken cancellationToken = default);
}
