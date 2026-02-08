using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Update;

internal sealed class UpdateDecorationCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdateDecorationCommand, Result<DecorationResponse>>
{
    public async ValueTask<Result<DecorationResponse>> Handle(UpdateDecorationCommand command, CancellationToken cancellationToken)
    {
        var decoration = await dbContext.Decorations
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);

        if (decoration is null)
        {
            return Result.Failure<DecorationResponse>(DecorationErrors.NotFound(command.Id));
        }

        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingDecoration = await dbContext.Decorations
            .FirstOrDefaultAsync(d => d.Name.ToUpper() == nameToMatch && d.Id != command.Id,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingDecoration is not null)
        {
            return Result.Failure<DecorationResponse>(DecorationErrors.AlreadyExists(command.Name));
        }

        var updateResult = decoration.Update(
            command.Name,
            command.Description,
            command.Category,
            command.ImageUrl,
            command.ImageIsometricUrl);

        if (updateResult.IsFailure)
        {
            return Result.Failure<DecorationResponse>(updateResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DecorationResponse
        {
            Id = updateResult.Value.Id,
            Name = updateResult.Value.Name,
            Description = updateResult.Value.Description,
            Category = updateResult.Value.Category,
            ImageUrl = updateResult.Value.ImageUrl,
            ImageIsometricUrl = updateResult.Value.ImageIsometricUrl
        };

        return Result.Success(response);
    }
}
