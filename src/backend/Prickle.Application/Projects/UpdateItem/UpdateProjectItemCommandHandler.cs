using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.UpdateItem;

internal sealed class UpdateProjectItemCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdateProjectItemCommand, Result<ProjectItemResponse>>
{
    public async ValueTask<Result<ProjectItemResponse>> Handle(UpdateProjectItemCommand command, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectItemResponse>(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure<ProjectItemResponse>(ProjectErrors.UserNotOwner(command.UserId));
        }

        var item = project.Items.FirstOrDefault(i => i.Id == command.ItemId);
        if (item is null)
        {
            return Result.Failure<ProjectItemResponse>(ProjectErrors.ProjectItemNotFound(command.ItemId));
        }

        var result = item.UpdatePosition(command.PosX, command.PosY, command.PosZ);
        if (result.IsFailure)
        {
            return Result.Failure<ProjectItemResponse>(result.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ProjectItemResponse
        {
            Id = item.Id,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            PosX = item.PosX,
            PosY = item.PosY,
            PosZ = item.PosZ
        };

        return Result.Success(response);
    }
}