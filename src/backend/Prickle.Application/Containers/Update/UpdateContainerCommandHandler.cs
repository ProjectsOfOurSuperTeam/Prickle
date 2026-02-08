using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;

namespace Prickle.Application.Containers.Update;

internal sealed class UpdateContainerCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdateContainerCommand, Result<ContainerResponse>>
{
    public async ValueTask<Result<ContainerResponse>> Handle(UpdateContainerCommand command, CancellationToken cancellationToken)
    {
        var container = await dbContext.Containers
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (container is null)
        {
            return Result.Failure<ContainerResponse>(ContainerErrors.NotFound(command.Id));
        }

        // Check if another container with the same name exists (excluding current container)
        var nameToMatch = command.Name.ToUpperInvariant().Trim();
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingContainer = await dbContext.Containers
            .FirstOrDefaultAsync(c => c.Name.ToUpper() == nameToMatch && c.Id != command.Id,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingContainer is not null)
        {
            return Result.Failure<ContainerResponse>(ContainerErrors.AlreadyExists(command.Name));
        }

        var updateResult = container.Update(
            command.Name,
            command.Description,
            command.Volume,
            command.IsClosed,
            command.ImageUrl,
            command.ImageIsometricUrl);

        if (updateResult.IsFailure)
        {
            return Result.Failure<ContainerResponse>(updateResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ContainerResponse
        {
            Id = container.Id,
            Name = container.Name,
            Description = container.Description,
            Volume = container.Volume,
            IsClosed = container.IsClosed,
            ImageUrl = container.ImageUrl,
            ImageIsometricUrl = container.ImageIsometricUrl
        };

        return Result.Success(response);
    }
}