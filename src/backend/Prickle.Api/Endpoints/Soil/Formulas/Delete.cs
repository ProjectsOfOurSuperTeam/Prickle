using Prickle.Application.Soil.Formulas.Delete;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeleteSoilFormula";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Soil.Formulas.Delete,
            async (
                [FromRoute] Guid id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteSoilFormulaCommand(id);
                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    () => Results.NoContent(),
                    CustomResults.Problem
                );
            })
            .WithName(EndpointName)
            .WithTags(Tags.Soil, Tags.SoilFormulas)
            .WithDescription("Deletes an existing soil formula identified by its ID.")
            .WithSummary("Delete a soil formula")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .HasPermission(AuthorizationPolicies.Admin);
    }
}
