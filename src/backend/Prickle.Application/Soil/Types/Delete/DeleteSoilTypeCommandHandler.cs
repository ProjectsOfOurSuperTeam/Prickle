
using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Types.Delete;

internal sealed class DeleteSoilTypeCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeleteSoilTypeCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSoilTypeCommand command, CancellationToken cancellationToken)
    {
        var soilType = await dbContext.SoilTypes.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (soilType is null)
        {
            return Result.Failure(SoilErrors.SoilType.NotFound(command.Id));
        }

        dbContext.SoilTypes.Remove(soilType);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
