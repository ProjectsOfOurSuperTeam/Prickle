using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.Delete;

internal sealed class DeleteProjectCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeleteProjectCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (project is null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.Id));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure(ProjectErrors.UserNotOwner(command.UserId));
        }

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}