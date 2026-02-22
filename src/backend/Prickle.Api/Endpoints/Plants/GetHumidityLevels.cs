using Prickle.Application.Plants.GetHumidityLevels;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetHumidityLevels : IEndpoint
{
    public const string EndpointName = "GetPlantHumidityLevels";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetHumidityLevels, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantHumidityLevelsQuery(), cancellationToken);
            return result.Match(
                humidityLevels => Results.Ok(humidityLevels),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves a list of all plant humidity levels.")
        .WithSummary("Get all plant humidity levels")
        .Produces<PlantHumidityLevelsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}