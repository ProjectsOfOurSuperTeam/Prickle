using Prickle.Application.Decorations;
using Prickle.Application.Decorations.Update;
using Prickle.Domain.Decorations;
using Prickle.Infrastructure.Authentication;
using SharedKernel;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateDecorationRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required int Category { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
    }
    public const string EndpointName = "UpdateDecoration";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Decorations.Update,
            async (
                [FromRoute] Guid id,
                UpdateDecorationRequest request,
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

                var command = new UpdateDecorationCommand(
                    id,
                    request.Name,
                    request.Description,
                    category,
                    request.ImageUrl,
                    request.ImageIsometricUrl);

                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    decorationResponse => Results.Ok(decorationResponse),
                    CustomResults.Problem
                );
            })
            .WithName(EndpointName)
            .WithTags(Tags.Decorations)
            .WithDescription("Updates an existing decoration identified by its ID.")
            .WithSummary("Update a decoration")
            .Accepts<UpdateDecorationRequest>("application/json")
            .Produces<DecorationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .HasPermission(AuthorizationPolicies.Admin);
    }
}