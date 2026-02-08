
using Prickle.Application.Decorations.Delete;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeleteDecoration";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Decorations.Delete, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeleteDecorationCommand(id), cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Decorations)
        .WithSummary("Deletes a decoration by its ID.")
        .WithDescription("Deletes a decoration by its ID. Returns 204 No Content if the deletion is successful, or 400 Bad Request if the request is invalid.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

    }
}
