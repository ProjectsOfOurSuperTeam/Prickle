using Prickle.Application.Plants.GetItemSizes;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetItemSizes : IEndpoint
{
    public const string EndpointName = "GetPlantItemSizes";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetItemSizes, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPlantItemSizesQuery(), cancellationToken);
            return result.Match(
                itemSizes => Results.Ok(itemSizes),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves a list of all plant item sizes.")
        .WithSummary("Get all plant item sizes")
        .Produces<PlantItemSizesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}