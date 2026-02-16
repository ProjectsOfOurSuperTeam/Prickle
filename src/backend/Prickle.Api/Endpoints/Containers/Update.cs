using Prickle.Application.Containers;
using Prickle.Application.Containers.Update;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Containers;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateContainerRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required float Volume { get; init; }
        public required bool IsClosed { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
    }

    public const string EndpointName = "UpdateContainer";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Containers.Update, async (
            [FromRoute] Guid id,
            [FromBody] UpdateContainerRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new UpdateContainerCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.Volume,
                    request.IsClosed,
                    request.ImageUrl,
                    request.ImageIsometricUrl
                ),
                cancellationToken);

            return result.Match(
                container => Results.Ok(container),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Containers)
        .WithSummary("Updates an existing container.")
        .WithDescription("Updates an existing container.")
        .Accepts<UpdateContainerRequest>("application/json")
        .Produces<ContainerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermission(AuthorizationPolicies.Admin);
    }
}