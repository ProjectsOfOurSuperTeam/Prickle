using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.GetAll;

internal sealed class GetAllProjectsQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetAllProjectsQuery, Result<ProjectsResponse>>
{
    public async ValueTask<Result<ProjectsResponse>> Handle(GetAllProjectsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Project> projectsQuery = dbContext.Projects
            .Include(p => p.Items)
            .AsNoTracking();

        if (query.UserId.HasValue)
        {
            projectsQuery = projectsQuery.Where(p => p.UserId == query.UserId.Value);
        }

        if (query.IsPublished.HasValue)
        {
            projectsQuery = projectsQuery.Where(p => p.IsPublished == query.IsPublished.Value);
        }

        projectsQuery = projectsQuery.Where(p => p.IsPublished || p.UserId == query.CallerUserId);

        if (!string.IsNullOrWhiteSpace(query.SortField))
        {
            var ascending = query.SortOrder is null || query.SortOrder == SortOrder.Ascending;
            projectsQuery = query.SortField.ToLowerInvariant() switch
            {
                "createdat" => ascending
                    ? projectsQuery.OrderBy(p => p.CreatedAt)
                    : projectsQuery.OrderByDescending(p => p.CreatedAt),
                _ => ascending
                    ? projectsQuery.OrderBy(p => p.Id)
                    : projectsQuery.OrderByDescending(p => p.Id)
            };
        }
        else
        {
            projectsQuery = projectsQuery.OrderByDescending(p => p.CreatedAt);
        }

        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var pageOffset = (query.Page > 0 ? query.Page - 1 : 0) * pageSize;

        var totalCount = await projectsQuery.CountAsync(cancellationToken);
        var projects = await projectsQuery
            .Skip(pageOffset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            UserId = p.UserId,
            ContainerId = p.ContainerId,
            Preview = p.Preview,
            CreatedAt = p.CreatedAt,
            IsPublished = p.IsPublished,
            Items = p.Items.Select(i => new ProjectItemResponse
            {
                Id = i.Id,
                ItemType = i.ItemType,
                ItemId = i.ItemId,
                PosX = i.PosX,
                PosY = i.PosY,
                PosZ = i.PosZ
            }).ToList()
        }).ToList();

        return Result.Success(new ProjectsResponse
        {
            Items = items,
            Total = totalCount,
            Page = query.Page,
            PageSize = pageSize
        });
    }
}