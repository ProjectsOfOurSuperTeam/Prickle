using Prickle.Application.Plants;
using Prickle.Application.Plants.Get;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetPlant";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.Get, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantQuery(id), cancellationToken);
            return result.Match(
                plant => Results.Ok(plant),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves a plant by its ID.")
        .WithSummary("Get a plant by ID")
        .Produces<PlantResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}