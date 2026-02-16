using Prickle.Application.Soil.Types.Delete;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Soil.Types;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeleteSoilType";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Soil.Types.Delete,
            async (
                [FromRoute] int id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteSoilTypeCommand(id);
                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    () => Results.NoContent(),
                    CustomResults.Problem
                );
            })
            .WithName(EndpointName)
            .WithTags(Tags.Soil, Tags.SoilTypes)
            .WithDescription("Deletes an existing soil type identified by its ID.")
            .WithSummary("Delete a soil type.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .HasPermission(AuthorizationPolicies.Admin);
    }
}
