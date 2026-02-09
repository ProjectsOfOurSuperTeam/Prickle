using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.AddItem;

internal sealed class AddProjectItemCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddProjectItemCommand, Result<ProjectItemResponse>>
{
    public async ValueTask<Result<ProjectItemResponse>> Handle(AddProjectItemCommand command, CancellationToken cancellationToken)
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

        var itemResult = project.AddItem(
            command.ItemType,
            command.ItemId,
            command.PosX,
            command.PosY,
            command.PosZ);

        if (itemResult.IsFailure)
        {
            return Result.Failure<ProjectItemResponse>(itemResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var item = itemResult.Value;
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