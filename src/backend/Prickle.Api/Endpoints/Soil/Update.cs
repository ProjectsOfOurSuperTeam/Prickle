
using Prickle.Application.Soil.Types;
using Prickle.Application.Soil.Types.Update;

namespace Prickle.Api.Endpoints.Soil;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateSoilTypeRequest(string NewName);
    public const string EndpointName = "UpdateSoilType";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Soil.Update,
            async (
                [FromRoute] int id,
                UpdateSoilTypeRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateSoilTypeCommand(id, request.NewName);
                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    soilTypeResponse => Results.Ok(soilTypeResponse),
                    CustomResults.Problem
                );
            })
            .WithName(EndpointName)
            .WithTags(Tags.Soil)
            .WithDescription("Updates the name of an existing soil type identified by its ID.")
            .WithSummary("Update a soil type.")
            .Produces<SoilTypeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
        //TODO
    }
}
