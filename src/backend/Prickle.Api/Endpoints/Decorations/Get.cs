
using Prickle.Application.Decorations;
using Prickle.Application.Decorations.Get;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetDecoration";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Decorations.Get, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetDecorationQuery(id), cancellationToken);
            return result.Match(
                decoration => Results.Ok(decoration),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Decorations)
        .WithDescription("Retrieves a decoration by its ID.")
        .WithSummary("Get a decoration by ID")
        .Produces<DecorationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
