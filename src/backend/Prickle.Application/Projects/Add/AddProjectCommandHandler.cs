using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.Add;

internal sealed class AddProjectCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddProjectCommand, Result<ProjectResponse>>
{
    public async ValueTask<Result<ProjectResponse>> Handle(AddProjectCommand command, CancellationToken cancellationToken)
    {
        var containerExists = await dbContext.Containers
            .AnyAsync(c => c.Id == command.ContainerId, cancellationToken);

        if (!containerExists)
        {
            return Result.Failure<ProjectResponse>(
                ContainerErrors.NotFound(command.ContainerId));
        }

        var result = Project.Create(
            command.UserId,
            command.ContainerId,
            command.Preview);

        if (result.IsFailure)
        {
            return Result.Failure<ProjectResponse>(result.Error);
        }

        var project = result.Value;
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(project.ToResponse());
    }
}