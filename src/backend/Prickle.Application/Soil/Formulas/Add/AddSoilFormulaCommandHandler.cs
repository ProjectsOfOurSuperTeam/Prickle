using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Application.Extensions;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Formulas.Add;

internal sealed class AddSoilFormulaCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<AddSoilFormulaCommand, Result<SoilFormulaResponse>>
{
    public async ValueTask<Result<SoilFormulaResponse>> Handle(AddSoilFormulaCommand command,
        CancellationToken cancellationToken)
    {
        var nameToMatch = command.Name.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingSoilFormula = await dbContext.SoilFormulas
            .FirstOrDefaultAsync(soilFormula => soilFormula.Name.ToUpper() == nameToMatch,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingSoilFormula is not null)
        {
            return Result.Failure<SoilFormulaResponse>(SoilErrors.SoilFormulas.AlreadyExists(command.Name));
        }

        var formulaItemsResult = command.FormulaItemDTOs.ToSoilFormulaItems();
        if (formulaItemsResult.IsFailure)
        {
            return Result.Failure<SoilFormulaResponse>(formulaItemsResult.Error);
        }

        var soilFormulaResult = SoilFormulas.Create(command.Name, formulaItemsResult.Value);
        if (soilFormulaResult.IsFailure)
        {
            return Result.Failure<SoilFormulaResponse>(soilFormulaResult.Error);
        }

        var dbResult = dbContext.SoilFormulas.Add(soilFormulaResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (dbResult is null)
        {
            return Result.Failure<SoilFormulaResponse>(SoilErrors.SoilFormulas.FailedToCreate(command.Name));
        }

        var soilTypeIds = dbResult.Entity.Formula.Select(f => f.SoilTypeId).Distinct().ToList();
        var soilTypes = await dbContext.SoilTypes
            .AsNoTracking()
            .Where(st => soilTypeIds.Contains(st.Id))
            .ToListAsync(cancellationToken);

        var items = dbResult.Entity.Formula
            .Select(f => new SoilFormulaItemResponse(
                new Types.SoilTypeResponse(f.SoilTypeId, soilTypes.First(st => st.Id == f.SoilTypeId).Name),
                f.Percentage,
                f.Order))
            .ToList();

        var response = new SoilFormulaResponse
        (
            dbResult.Entity.Id,
            dbResult.Entity.Name,
            items
        );

        return Result.Success(response);
    }
}
