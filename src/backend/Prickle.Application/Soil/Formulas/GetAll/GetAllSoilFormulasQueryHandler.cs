using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Formulas.GetAll;

internal sealed class GetAllSoilFormulasQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllSoilFormulasQuery, Result<SoilFormulasResponse>>
{
    public async ValueTask<Result<SoilFormulasResponse>> Handle(GetAllSoilFormulasQuery query, CancellationToken cancellationToken)
    {
        IQueryable<SoilFormulas> soilFormulasQuery = dbContext.SoilFormulas
            .AsNoTracking()
            .Include(sf => sf.Formula);

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            soilFormulasQuery = soilFormulasQuery.Where(sf => sf.Name.ToUpper().Contains(query.Name.ToUpperInvariant()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        // Filter by soil type IDs - show only formulas that contain ALL specified soil types
        if (query.SoilTypeIds is not null && query.SoilTypeIds.Any())
        {
            var soilTypeIdsList = query.SoilTypeIds.ToList();
            soilFormulasQuery = soilFormulasQuery.Where(sf =>
                soilTypeIdsList.All(soilTypeId =>
                    sf.Formula.Any(f => f.SoilTypeId == soilTypeId)));
        }

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            soilFormulasQuery = query.SortField.ToLowerInvariant() switch
            {
                "name" => ascending
                    ? soilFormulasQuery.OrderBy(sf => sf.Name)
                    : soilFormulasQuery.OrderByDescending(sf => sf.Name),
                "itemcount" => ascending
                    ? soilFormulasQuery.OrderBy(sf => sf.Formula.Count)
                    : soilFormulasQuery.OrderByDescending(sf => sf.Formula.Count),
                _ => ascending
                    ? soilFormulasQuery.OrderBy(sf => sf.Id)
                    : soilFormulasQuery.OrderByDescending(sf => sf.Id)
            };
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await soilFormulasQuery.CountAsync(cancellationToken);
        var soilFormulas = await soilFormulasQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get all soil type IDs from all formulas
        var allSoilTypeIds = soilFormulas
            .SelectMany(sf => sf.Formula.Select(f => f.SoilTypeId))
            .Distinct()
            .ToList();

        // Fetch all soil types at once
        var soilTypes = await dbContext.SoilTypes
            .AsNoTracking()
            .Where(st => allSoilTypeIds.Contains(st.Id))
            .ToListAsync(cancellationToken);

        // Map to response
        var items = soilFormulas.Select(sf =>
        {
            var formulaItems = sf.Formula
                .Select(f => new SoilFormulaItemResponse(
                    new Types.SoilTypeResponse(f.SoilTypeId, soilTypes.First(st => st.Id == f.SoilTypeId).Name),
                    f.Percentage,
                    f.Order))
                .ToList();

            return new SoilFormulaResponse(sf.Id, sf.Name, formulaItems);
        }).ToList();

        return Result.Success(new SoilFormulasResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });
    }
}
