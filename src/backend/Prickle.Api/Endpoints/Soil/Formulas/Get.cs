
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Formulas.Get;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetSoilFormula";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.Formulas.Get, async (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSoilFormulaByIdQuery(id), cancellationToken);

            return result.Match(
                soilFormulaResponse => Results.Ok(soilFormulaResponse),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil, Tags.SoilFormulas)
        .WithDescription("Retrieves a specific soil formula by its ID.")
        .WithSummary("Get a specific soil formula")
        .Produces<SoilFormulaResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermission(AuthorizationPolicies.User);
    }
}
