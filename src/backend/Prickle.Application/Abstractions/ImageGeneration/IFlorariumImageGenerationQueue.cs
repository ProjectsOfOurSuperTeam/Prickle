namespace Prickle.Application.Abstractions.ImageGeneration;

public interface IFlorariumImageGenerationQueue
{
    void Enqueue(Guid projectId);
}