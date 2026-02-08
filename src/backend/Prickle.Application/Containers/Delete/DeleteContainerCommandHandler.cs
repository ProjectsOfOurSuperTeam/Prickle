using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;

namespace Prickle.Application.Containers.Delete;

internal sealed class DeleteContainerCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeleteContainerCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteContainerCommand command, CancellationToken cancellationToken)
    {
        var container = await dbContext.Containers
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (container is null)
        {
            return Result.Failure(ContainerErrors.NotFound(command.Id));
        }

        dbContext.Containers.Remove(container);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}