using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Formulas.Get;

internal sealed class GetSoilFormulaByIdQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetSoilFormulaByIdQuery, Result<SoilFormulaResponse>>
{
    public async ValueTask<Result<SoilFormulaResponse>> Handle(GetSoilFormulaByIdQuery query, CancellationToken cancellationToken)
    {
        var soilFormula = await dbContext.SoilFormulas
            .AsNoTracking()
            .Include(sf => sf.Formula)
            .FirstOrDefaultAsync(sf => sf.Id == query.Id, cancellationToken);

        if (soilFormula is null)
        {
            return Result.Failure<SoilFormulaResponse>(SoilErrors.SoilFormulas.NotFound(query.Id));
        }

        var soilTypeIds = soilFormula.Formula.Select(f => f.SoilTypeId).ToList();
        var soilTypes = await dbContext.SoilTypes
            .AsNoTracking()
            .Where(st => soilTypeIds.Contains(st.Id))
            .ToListAsync(cancellationToken);

        var items = soilFormula.Formula
            .Select(f => new SoilFormulaItemResponse(
                new Types.SoilTypeResponse(f.SoilTypeId, soilTypes.First(st => st.Id == f.SoilTypeId).Name),
                f.Percentage,
                f.Order))
            .ToList();

        var response = new SoilFormulaResponse(
            soilFormula.Id,
            soilFormula.Name,
            items
        );

        return Result.Success(response);
    }
}
