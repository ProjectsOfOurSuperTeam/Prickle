using Prickle.Application.Containers;
using Prickle.Application.Containers.Add;

namespace Prickle.Api.Endpoints.Containers;

internal sealed class Add : IEndpoint
{
    public sealed record AddContainerRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required float Volume { get; init; }
        public required bool IsClosed { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
    }

    public const string EndpointName = "AddContainer";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Containers.Add, async (
            [FromBody] AddContainerRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new AddContainerCommand(
                    request.Name,
                    request.Description,
                    request.Volume,
                    request.IsClosed,
                    request.ImageUrl,
                    request.ImageIsometricUrl
                ),
                cancellationToken);

            return result.Match(
                (container) => Results.CreatedAtRoute(Get.EndpointName, new { id = container.Id }, container),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Containers)
        .WithSummary("Adds a new container.")
        .WithDescription("Adds a new container.")
        .Accepts<AddContainerRequest>("application/json")
        .Produces<ContainerResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
    }
}