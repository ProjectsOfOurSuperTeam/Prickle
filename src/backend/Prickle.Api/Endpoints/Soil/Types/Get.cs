using Prickle.Application.Soil.Types;
using Prickle.Application.Soil.Types.Get;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Soil.Types;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetSoilTypes";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.Types.Get, async (
            [FromRoute] int id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSoilTypeQuery(id), cancellationToken);

            return result.Match(
                soilTypeResponse => Results.Ok(soilTypeResponse),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil, Tags.SoilTypes)
        .WithDescription("Retrieves a specific soil type by its ID.")
        .WithSummary("Get a specific soil type.")
        .Produces<SoilTypeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}
