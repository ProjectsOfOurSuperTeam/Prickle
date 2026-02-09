using Prickle.Application.Plants;
using Prickle.Application.Plants.Delete;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeletePlant";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Plants.Delete, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeletePlantCommand(id), cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithSummary("Deletes a plant by its ID.")
        .WithDescription("Deletes a plant by its ID. Returns 204 No Content if the deletion is successful, or 400 Bad Request if the request is invalid.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }
}