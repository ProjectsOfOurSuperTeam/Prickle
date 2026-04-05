using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects.Get;

internal sealed class GetProjectQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetProjectQuery, Result<ProjectResponse>>
{
    public async ValueTask<Result<ProjectResponse>> Handle(GetProjectQuery query, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.NotFound(query.Id));
        }

        if (!project.IsPublished && project.UserId != query.UserId)
        {
            return Result.Failure<ProjectResponse>(ProjectErrors.UserNotOwner(query.UserId));
        }

        return Result.Success(project.ToResponse());
    }
}