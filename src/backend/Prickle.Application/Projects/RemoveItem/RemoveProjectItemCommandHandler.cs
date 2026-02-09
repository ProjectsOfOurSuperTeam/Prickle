using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.RemoveItem;

internal sealed class RemoveProjectItemCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<RemoveProjectItemCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveProjectItemCommand command, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure(ProjectErrors.UserNotOwner(command.UserId));
        }

        var result = project.RemoveItem(command.ItemId);
        if (result.IsFailure)
        {
            return result;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}