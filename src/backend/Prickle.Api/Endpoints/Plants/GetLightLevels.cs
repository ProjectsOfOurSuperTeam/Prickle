using Prickle.Application.Plants.GetLightLevels;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetLightLevels : IEndpoint
{
    public const string EndpointName = "GetPlantLightLevels";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetLightLevels, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantLightLevelsQuery(), cancellationToken);
            return result.Match(
                lightLevels => Results.Ok(lightLevels),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves a list of all plant light levels.")
        .WithSummary("Get all plant light levels")
        .Produces<PlantLightLevelsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}