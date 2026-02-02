using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Types.GetAll;

internal sealed class GetAllSoilTypeQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllSoilTypeQuery, Result<SoilTypesResponse>>
{
    public async ValueTask<Result<SoilTypesResponse>> Handle(GetAllSoilTypeQuery query, CancellationToken cancellationToken)
    {
        IQueryable<SoilType> soilTypesQuery = dbContext.SoilTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            soilTypesQuery = soilTypesQuery.Where(c => c.Name.ToLower().Contains(query.Name.ToLowerInvariant()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        }

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            soilTypesQuery = query.SortField.ToLowerInvariant() switch
            {
                "name" => ascending
                    ? soilTypesQuery.OrderBy(st => st.Name)
                    : soilTypesQuery.OrderByDescending(st => st.Name),
                _ => ascending
                    ? soilTypesQuery.OrderBy(st => st.Id)
                    : soilTypesQuery.OrderByDescending(st => st.Id)
            };
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await soilTypesQuery.CountAsync(cancellationToken);
        var items = await soilTypesQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .Select(st => new SoilTypeResponse(
                st.Id,
                st.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(new SoilTypesResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });

    }
}
