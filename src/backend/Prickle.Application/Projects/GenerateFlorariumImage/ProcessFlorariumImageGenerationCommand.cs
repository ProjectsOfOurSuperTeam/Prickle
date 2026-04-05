namespace Prickle.Application.Projects.GenerateFlorariumImage;

public sealed record ProcessFlorariumImageGenerationCommand(Guid ProjectId) : ICommand<Result>;