using Prickle.Application.Decorations.GetCategories;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class GetCategories : IEndpoint
{
    public const string EndpointName = "GetCategories";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Decorations.GetCategories, async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetDecorationCategoriesQuery(), cancellationToken);
            return result.Match(
                categories => Results.Ok(categories),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Decorations)
        .WithDescription("Retrieves a list of all decoration categories.")
        .WithSummary("Get all decoration categories")
        .Produces<DecorationCategoriesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}
