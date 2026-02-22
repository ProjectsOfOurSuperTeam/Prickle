using Prickle.Application.Containers;
using Prickle.Application.Containers.Get;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Containers;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetContainer";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Containers.Get, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetContainerQuery(id), cancellationToken);
            return result.Match(
                container => Results.Ok(container),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Containers)
        .WithDescription("Retrieves a container by its ID.")
        .WithSummary("Get a container by ID")
        .Produces<ContainerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}