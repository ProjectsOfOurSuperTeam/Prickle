using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.Add;

internal sealed class AddPlantCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddPlantCommand, Result<PlantResponse>>
{
    public async ValueTask<Result<PlantResponse>> Handle(AddPlantCommand command, CancellationToken cancellationToken)
    {
        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingPlant = await dbContext.Plants
            .FirstOrDefaultAsync(plant => plant.Name.ToUpper() == nameToMatch,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingPlant is not null)
        {
            return Result.Failure<PlantResponse>(PlantErrors.AlreadyExists(command.Name));
        }

        var result = Plant.Create(
            command.Name,
            command.NameLatin,
            command.Description,
            command.ImageUrl,
            command.ImageIsometricUrl,
            command.Category,
            command.LightLevel,
            command.WaterNeed,
            command.HumidityLevel,
            command.ItemMaxSize,
            command.SoilFormulaId);

        if (result.IsFailure)
        {
            return Result.Failure<PlantResponse>(result.Error);
        }

        var plant = result.Value;
        dbContext.Plants.Add(plant);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new PlantResponse
        {
            Id = plant.Id,
            Name = plant.Name,
            NameLatin = plant.NameLatin,
            Description = plant.Description,
            ImageUrl = plant.ImageUrl,
            ImageIsometricUrl = plant.ImageIsometricUrl,
            Category = plant.Category,
            LightLevel = plant.LightLevel,
            WaterNeed = plant.WaterNeed,
            HumidityLevel = plant.HumidityLevel,
            ItemMaxSize = plant.ItemMaxSize,
            SoilFormulaId = plant.SoilFormulaId
        };

        return Result.Success(response);
    }
}