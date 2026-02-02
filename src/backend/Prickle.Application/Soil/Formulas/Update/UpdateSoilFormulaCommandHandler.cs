using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Application.Extensions;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Formulas.Update;

internal sealed class UpdateSoilFormulaCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<UpdateSoilFormulaCommand, Result<SoilFormulaResponse>>
{
    public async ValueTask<Result<SoilFormulaResponse>> Handle(UpdateSoilFormulaCommand command, CancellationToken cancellationToken)
    {
        var soilFormula = await dbContext.SoilFormulas
            .Include(sf => sf.Formula)
            .FirstOrDefaultAsync(sf => sf.Id == command.Id, cancellationToken);

        if (soilFormula is null)
        {
            return Result.Failure<SoilFormulaResponse>(SoilErrors.SoilFormulas.NotFound(command.Id));
        }

        var nameToMatch = command.NewName.ToUpperInvariant().Trim();

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        var existingSoilFormula = await dbContext.SoilFormulas
            .FirstOrDefaultAsync(sf => sf.Name.ToUpper() == nameToMatch && sf.Id != command.Id,
                cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        if (existingSoilFormula is not null)
        {
            return Result.Failure<SoilFormulaResponse>(SoilErrors.SoilFormulas.AlreadyExists(command.NewName));
        }

        var formulaItemsResult = command.FormulaItemDTOs.ToSoilFormulaItems();
        if (formulaItemsResult.IsFailure)
        {
            return Result.Failure<SoilFormulaResponse>(formulaItemsResult.Error);
        }

        var updateResult = soilFormula.Update(command.NewName, formulaItemsResult.Value);
        if (updateResult.IsFailure)
        {
            return Result.Failure<SoilFormulaResponse>(updateResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var soilTypeIds = updateResult.Value.Formula.Select(f => f.SoilTypeId).Distinct().ToList();
        var soilTypes = await dbContext.SoilTypes
            .AsNoTracking()
            .Where(st => soilTypeIds.Contains(st.Id))
            .ToListAsync(cancellationToken);

        var items = updateResult.Value.Formula
            .Select(f => new SoilFormulaItemResponse(
                new Types.SoilTypeResponse(f.SoilTypeId, soilTypes.First(st => st.Id == f.SoilTypeId).Name),
                f.Percentage,
                f.Order))
            .ToList();

        var response = new SoilFormulaResponse
        (
            updateResult.Value.Id,
            updateResult.Value.Name,
            items
        );

        return Result.Success(response);
    }
}