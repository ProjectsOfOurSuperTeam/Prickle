using Prickle.Application.Containers.Delete;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Containers;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeleteContainer";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Containers.Delete, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeleteContainerCommand(id), cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Containers)
        .WithSummary("Deletes a container.")
        .WithDescription("Deletes a container by its ID.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermission(AuthorizationPolicies.Admin);
    }
}