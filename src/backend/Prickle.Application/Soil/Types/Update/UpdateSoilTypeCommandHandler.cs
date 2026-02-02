using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Types.Update;

internal sealed class UpdateSoilTypeCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdateSoilTypeCommand, Result<SoilTypeResponse>>
{
    public async ValueTask<Result<SoilTypeResponse>> Handle(UpdateSoilTypeCommand command, CancellationToken cancellationToken)
    {
        var soilType = await dbContext.SoilTypes.FindAsync([command.Id], cancellationToken);
        if (soilType is null)
        {
            return Result.Failure<SoilTypeResponse>(SoilErrors.SoilType.NotFound(command.Id));
        }

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var nameToMatch = command.NewName.ToUpperInvariant();
        var existingSoilType = await dbContext.SoilTypes
            .FirstOrDefaultAsync(st => st.Name.ToUpper() == nameToMatch && st.Id != command.Id, cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingSoilType is not null)
        {
            return Result.Failure<SoilTypeResponse>(SoilErrors.SoilType.AlreadyExists(command.NewName));
        }

        var updateResult = soilType.Update(command.NewName);
        if (updateResult.IsFailure)
        {
            return Result.Failure<SoilTypeResponse>(updateResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new SoilTypeResponse(updateResult.Value.Id, updateResult.Value.Name));
    }
}
