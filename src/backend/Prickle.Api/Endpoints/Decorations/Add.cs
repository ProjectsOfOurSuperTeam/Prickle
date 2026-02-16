using Prickle.Application.Decorations;
using Prickle.Application.Decorations.Add;
using Prickle.Domain.Decorations;
using Prickle.Infrastructure.Authentication;
using SharedKernel;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class Add : IEndpoint
{
    public sealed record AddDecorationRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required int Category { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
    }
    public const string EndpointName = "AddDecoration";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Decorations.Add, async (
            [FromBody] AddDecorationRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var category = DecorationCategory.NoCategory;

            if (!DecorationCategory.TryFromValue(request.Category, out category))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            DecorationErrors.InvalidCategory(request.Category)
                            )
                        )
                    );
            }

            var result = await mediator.Send(
                new AddDecorationCommand(
                    request.Name,
                    request.Description,
                    category,
                    request.ImageUrl,
                    request.ImageIsometricUrl
                ),
                cancellationToken);

            return result.Match(
                   (decoration) => Results.CreatedAtRoute(Get.EndpointName, new { id = decoration.Id }, decoration),
                   CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Decorations)
        .WithSummary("Adds a new decoration.")
        .WithDescription("Adds a new decoration.")
        .Accepts<AddDecorationRequest>("application/json")
        .Produces<DecorationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermission(AuthorizationPolicies.Admin);
    }
}
