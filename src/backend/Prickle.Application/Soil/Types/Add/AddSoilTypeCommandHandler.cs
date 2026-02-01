using Mediator;
using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Types.Add;

internal sealed class AddSoilTypeCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddSoilTypeCommand, Result<SoilTypeResponse>>
{
    public async ValueTask<Result<SoilTypeResponse>> Handle(AddSoilTypeCommand command,
        CancellationToken cancellationToken)
    {
        var nameToMatch = command.Name.Trim();

        var existingSoilType = await dbContext.SoilTypes
            .FirstOrDefaultAsync(soilType =>
                EF.Functions.Like(soilType.Name, nameToMatch),
                cancellationToken);

        if (existingSoilType is not null)
        {
            return Result.Failure<SoilTypeResponse>(SoilErrors.SoilType.SoilTypeAlreadyExists(command.Name));
        }

        var soilTypeResult = SoilType.Create(command.Name);
        if (soilTypeResult.IsFailure)
        {
            return Result.Failure<SoilTypeResponse>(soilTypeResult.Error);
        }

        var dbResult = dbContext.SoilTypes.Add(soilTypeResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (dbResult is null)
        {
            return Result.Failure<SoilTypeResponse>(SoilErrors.SoilType.FailedToCreate(command.Name));
        }

        var response = new SoilTypeResponse
        {
            Id = dbResult.Entity.Id,
            Name = dbResult.Entity.Name
        };

        return Result.Success(response);
    }
}
