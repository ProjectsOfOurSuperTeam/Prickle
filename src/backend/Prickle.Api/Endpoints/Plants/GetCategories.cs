using Prickle.Application.Plants.GetHumidityLevels;
using Prickle.Application.Plants.GetPlantCategories;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetCategories : IEndpoint
{
    public const string EndpointName = "GetPlantCategories";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetCategories, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantCategoriesQuery(), cancellationToken);
            return result.Match(
                categories => Results.Ok(categories),
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