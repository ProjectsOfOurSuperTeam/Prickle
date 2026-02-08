using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Add;

internal sealed class AddDecorationCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddDecorationCommand, Result<DecorationResponse>>
{
    public async ValueTask<Result<DecorationResponse>> Handle(AddDecorationCommand command, CancellationToken cancellationToken)
    {
        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingDecoration = await dbContext.Decorations
            .FirstOrDefaultAsync(decoration => decoration.Name.ToUpper() == nameToMatch,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingDecoration is not null)
        {
            return Result.Failure<DecorationResponse>(DecorationErrors.AlreadyExists(command.Name));
        }

        var result = Decoration.Create(
            command.Name,
            command.Description ?? string.Empty,
            command.Category,
            command.ImageUrl ?? string.Empty,
            command.ImageIsometricUrl ?? string.Empty);

        if (result.IsFailure)
        {
            return Result.Failure<DecorationResponse>(result.Error);
        }

        var decoration = result.Value;
        dbContext.Decorations.Add(decoration);
        await dbContext.SaveChangesAsync(cancellationToken);
        var response = new DecorationResponse
        {
            Id = decoration.Id,
            Name = decoration.Name,
            Description = decoration.Description,
            Category = decoration.Category,
            ImageUrl = decoration.ImageUrl,
            ImageIsometricUrl = decoration.ImageIsometricUrl
        };
        return Result.Success(response);
    }
}
