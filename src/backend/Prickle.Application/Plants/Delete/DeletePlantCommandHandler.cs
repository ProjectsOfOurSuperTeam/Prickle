using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.Delete;

internal sealed class DeletePlantCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeletePlantCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePlantCommand command, CancellationToken cancellationToken)
    {
        var plant = await dbContext.Plants
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (plant is null)
        {
            return Result.Failure(PlantErrors.NotFound(command.Id));
        }

        dbContext.Plants.Remove(plant);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}