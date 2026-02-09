using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.Update;

internal sealed class UpdatePlantCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdatePlantCommand, Result<PlantResponse>>
{
    public async ValueTask<Result<PlantResponse>> Handle(UpdatePlantCommand command, CancellationToken cancellationToken)
    {
        var plant = await dbContext.Plants
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (plant is null)
        {
            return Result.Failure<PlantResponse>(PlantErrors.NotFound(command.Id));
        }

        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingPlant = await dbContext.Plants
            .FirstOrDefaultAsync(p => p.Name.ToUpper() == nameToMatch && p.Id != command.Id,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingPlant is not null)
        {
            return Result.Failure<PlantResponse>(PlantErrors.AlreadyExists(command.Name));
        }

        var result = plant.Update(
            command.Name,
            command.NameLatin,
            command.Description,
            command.ImageUrl,
            command.ImageIsometricUrl,
            command.LightLevel,
            command.WaterNeed,
            command.HumidityLevel,
            command.ItemMaxSize,
            command.SoilFormulaId);

        if (result.IsFailure)
        {
            return Result.Failure<PlantResponse>(result.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new PlantResponse
        {
            Id = plant.Id,
            Name = plant.Name,
            NameLatin = plant.NameLatin,
            Description = plant.Description,
            ImageUrl = plant.ImageUrl,
            ImageIsometricUrl = plant.ImageIsometricUrl,
            LightLevel = plant.LightLevel,
            WaterNeed = plant.WaterNeed,
            HumidityLevel = plant.HumidityLevel,
            ItemMaxSize = plant.ItemMaxSize,
            SoilFormulaId = plant.SoilFormulaId
        };

        return Result.Success(response);
    }
}