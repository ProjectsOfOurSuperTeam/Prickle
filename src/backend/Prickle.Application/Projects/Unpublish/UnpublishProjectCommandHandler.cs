using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.Unpublish;

internal sealed class UnpublishProjectCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UnpublishProjectCommand, Result<ProjectResponse>>
{
    public async ValueTask<Result<ProjectResponse>> Handle(UnpublishProjectCommand command, CancellationToken cancellationToken)
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

        var result = project.Unpublish();
        if (result.IsFailure)
        {
            return Result.Failure<ProjectResponse>(result.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(project.ToResponse());
    }
}