using Prickle.Application.Plants.GetWaterNeeds;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetWaterNeeds : IEndpoint
{
    public const string EndpointName = "GetPlantWaterNeeds";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetWaterNeeds, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantWaterNeedsQuery(), cancellationToken);
            return result.Match(
                waterNeeds => Results.Ok(waterNeeds),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves a list of all plant water needs.")
        .WithSummary("Get all plant water needs")
        .Produces<PlantWaterNeedsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}