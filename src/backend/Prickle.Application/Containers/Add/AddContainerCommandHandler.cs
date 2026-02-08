using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;

namespace Prickle.Application.Containers.Add;

internal sealed class AddContainerCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddContainerCommand, Result<ContainerResponse>>
{
    public async ValueTask<Result<ContainerResponse>> Handle(AddContainerCommand command, CancellationToken cancellationToken)
    {
        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingContainer = await dbContext.Containers
            .FirstOrDefaultAsync(container => container.Name.ToUpper() == nameToMatch,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingContainer is not null)
        {
            return Result.Failure<ContainerResponse>(ContainerErrors.AlreadyExists(command.Name));
        }

        var result = Container.Create(
            command.Name,
            command.Description,
            command.Volume,
            command.IsClosed,
            command.ImageUrl,
            command.ImageIsometricUrl);

        if (result.IsFailure)
        {
            return Result.Failure<ContainerResponse>(result.Error);
        }

        var container = result.Value;
        dbContext.Containers.Add(container);
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