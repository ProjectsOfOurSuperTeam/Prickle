using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Plants;

namespace Prickle.Application.Plants.GetAll;

internal sealed class GetAllPlantsQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllPlantsQuery, Result<PlantsResponse>>
{
    public async ValueTask<Result<PlantsResponse>> Handle(GetAllPlantsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Plant> plantsQuery = dbContext.Plants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            plantsQuery = plantsQuery.Where(p => p.Name.ToUpper().Contains(query.Name.ToUpperInvariant()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            plantsQuery = query.SortField.ToLowerInvariant() switch
            {
                "name" => ascending
                    ? plantsQuery.OrderBy(p => p.Name)
                    : plantsQuery.OrderByDescending(p => p.Name),
                "namelatin" => ascending
                    ? plantsQuery.OrderBy(p => p.NameLatin)
                    : plantsQuery.OrderByDescending(p => p.NameLatin),
                _ => ascending
                    ? plantsQuery.OrderBy(p => p.Id)
                    : plantsQuery.OrderByDescending(p => p.Id)
            };
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await plantsQuery.CountAsync(cancellationToken);
        var items = await plantsQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .Select(p => new PlantResponse
            {
                Id = p.Id,
                Name = p.Name,
                NameLatin = p.NameLatin,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                ImageIsometricUrl = p.ImageIsometricUrl,
                LightLevel = p.LightLevel,
                WaterNeed = p.WaterNeed,
                HumidityLevel = p.HumidityLevel,
                ItemMaxSize = p.ItemMaxSize,
                SoilFormulaId = p.SoilFormulaId
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PlantsResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });
    }
}