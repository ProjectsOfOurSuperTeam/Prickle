
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Formulas.Add;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class Add : IEndpoint
{
    public sealed record AddSoilFormulaRequest(string Name, IEnumerable<SoilFormulaItemDTO> FormulaItems);
    public const string EndpointName = "AddSoilFormula";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Soil.Formulas.Add, async (
            [FromBody] AddSoilFormulaRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new AddSoilFormulaCommand(request.Name, request.FormulaItems),
                cancellationToken);

            return result.Match(
                   (soilFormula) => Results.CreatedAtRoute(Get.EndpointName, new { id = soilFormula.Id }, soilFormula),
                   CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil, Tags.SoilFormulas)
        .WithDescription("Adds a new soil formula to the system.")
        .WithSummary("Add a new soil formula")
        .Accepts<AddSoilFormulaRequest>("application/json")
        .Produces<SoilFormulaResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermission(AuthorizationPolicies.Admin);
    }
}
