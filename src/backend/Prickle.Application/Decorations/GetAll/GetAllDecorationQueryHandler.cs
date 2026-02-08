using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.GetAll;

internal sealed class GetAllDecorationQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllDecorationQuery, Result<DecorationsResponse>>
{
    public async ValueTask<Result<DecorationsResponse>> Handle(GetAllDecorationQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Decoration> decorationsQuery = dbContext.Decorations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

            decorationsQuery = decorationsQuery.Where(d => d.Name.ToUpper().Contains(query.Name.ToUpperInvariant()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons

        }

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            decorationsQuery = query.SortField.ToLowerInvariant() switch
            {
                "name" => ascending
                    ? decorationsQuery.OrderBy(d => d.Name)
                    : decorationsQuery.OrderByDescending(d => d.Name),
                _ => ascending
                    ? decorationsQuery.OrderBy(d => d.Id)
                    : decorationsQuery.OrderByDescending(d => d.Id)
            };
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await decorationsQuery.CountAsync(cancellationToken);
        var items = await decorationsQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .Select(d => new DecorationResponse
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Category = d.Category,
                ImageUrl = d.ImageUrl,
                ImageIsometricUrl = d.ImageIsometricUrl
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new DecorationsResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });
    }
}