using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Formulas.Update;

namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateSoilFormulaRequest(string NewName, IEnumerable<SoilFormulaItemDTO> FormulaItems);
    public const string EndpointName = "UpdateSoilFormula";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Soil.Formulas.Update,
            async (
                [FromRoute] Guid id,
                UpdateSoilFormulaRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateSoilFormulaCommand(id, request.NewName, request.FormulaItems);
                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    soilFormulaResponse => Results.Ok(soilFormulaResponse),
                    CustomResults.Problem
                );
            })
            .WithName(EndpointName)
            .WithTags(Tags.Soil, Tags.SoilFormulas)
            .WithDescription("Updates an existing soil formula identified by its ID.")
            .WithSummary("Update a soil formula")
            .Accepts<UpdateSoilFormulaRequest>("application/json")
            .Produces<SoilFormulaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
