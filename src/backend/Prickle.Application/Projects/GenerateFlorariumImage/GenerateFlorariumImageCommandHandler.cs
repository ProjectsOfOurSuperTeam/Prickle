using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Application.Abstractions.ImageGeneration;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.GenerateFlorariumImage;

internal sealed class GenerateFlorariumImageCommandHandler(
    IApplicationDbContext dbContext,
    IFlorariumImageGenerationQueue generationQueue)
    : ICommandHandler<GenerateFlorariumImageCommand, Result<ProjectResponse>>
{
    public async ValueTask<Result<ProjectResponse>> Handle(
        GenerateFlorariumImageCommand command,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.UserNotOwner(command.UserId));
        }

        var queueResult = project.QueueFlorariumImageGeneration(command.CanvasImage, command.ImageMimeType);
        if (queueResult.IsFailure)
        {
            return Result.Failure<ProjectResponse>(queueResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        generationQueue.Enqueue(project.Id);

        return Result.Success(project.ToResponse());
    }
}
