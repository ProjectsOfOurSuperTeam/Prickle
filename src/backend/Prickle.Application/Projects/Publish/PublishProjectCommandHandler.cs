using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.Publish;

internal sealed class PublishProjectCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<PublishProjectCommand, Result<ProjectResponse>>
{
    public async ValueTask<Result<ProjectResponse>> Handle(PublishProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.NotFound(command.Id));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.UserNotOwner(command.UserId));
        }

        var result = project.Publish();
        if (result.IsFailure)
        {
            return Result.Failure<ProjectResponse>(result.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ProjectResponse
        {
            Id = project.Id,
            UserId = project.UserId,
            ContainerId = project.ContainerId,
            Preview = project.Preview,
            CreatedAt = project.CreatedAt,
            IsPublished = project.IsPublished,
            Items = project.Items.Select(i => new ProjectItemResponse
            {
                Id = i.Id,
                ItemType = i.ItemType,
                ItemId = i.ItemId,
                PosX = i.PosX,
                PosY = i.PosY,
                PosZ = i.PosZ
            }).ToList()
        };

        return Result.Success(response);
    }
}