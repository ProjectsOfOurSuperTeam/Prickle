using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;

namespace Prickle.Application.Containers.Get;

internal sealed class GetContainerQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetContainerQuery, Result<ContainerResponse>>
{
    public async ValueTask<Result<ContainerResponse>> Handle(GetContainerQuery query, CancellationToken cancellationToken)
    {
        var container = await dbContext.Containers
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (container is null)
        {
            return Result.Failure<ContainerResponse>(ContainerErrors.NotFound(query.Id));
        }

        var response = new ContainerResponse
        {
            Id = container.Id,
            Name = container.Name,
            Description = container.Description,
            Volume = container.Volume,
            IsClosed = container.IsClosed,
            ImageUrl = container.ImageUrl,
            ImageIsometricUrl = container.ImageIsometricUrl
        };

        return Result.Success(response);
    }
}