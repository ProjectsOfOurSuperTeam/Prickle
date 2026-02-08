using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;

namespace Prickle.Application.Containers.GetAll;

internal sealed class GetAllContainersQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllContainersQuery, Result<ContainersResponse>>
{
    public async ValueTask<Result<ContainersResponse>> Handle(GetAllContainersQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Container> containersQuery = dbContext.Containers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            containersQuery = containersQuery.Where(c => c.Name.ToUpper().Contains(query.Name.ToUpperInvariant()));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            containersQuery = query.SortField.ToLowerInvariant() switch
            {
                "name" => ascending
                    ? containersQuery.OrderBy(c => c.Name)
                    : containersQuery.OrderByDescending(c => c.Name),
                "volume" => ascending
                    ? containersQuery.OrderBy(c => c.Volume)
                    : containersQuery.OrderByDescending(c => c.Volume),
                _ => ascending
                    ? containersQuery.OrderBy(c => c.Id)
                    : containersQuery.OrderByDescending(c => c.Id)
            };
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await containersQuery.CountAsync(cancellationToken);
        var items = await containersQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .Select(c => new ContainerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Volume = c.Volume,
                IsClosed = c.IsClosed,
                ImageUrl = c.ImageUrl,
                ImageIsometricUrl = c.ImageIsometricUrl
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new ContainersResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });
    }
}