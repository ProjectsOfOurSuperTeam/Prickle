using Prickle.Application.Projects;
using Prickle.Application.Projects.AddItem;
using Prickle.Domain.Projects;
using SharedKernel;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class AddItem : IEndpoint
{
    public sealed record AddProjectItemRequest
    {
        public required Guid UserId { get; init; }
        public required int ItemType { get; init; }
        public required Guid ItemId { get; init; }
        public required int PosX { get; init; }
        public required int PosY { get; init; }
        public required int PosZ { get; init; }
    }

    public const string EndpointName = "AddProjectItem";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.AddItem, async (
            [FromRoute] Guid id,
            [FromBody] AddProjectItemRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (!ProjectItemType.TryFromValue(request.ItemType, out var itemType))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            Error.Problem("ProjectItem.InvalidType", $"Invalid item type '{request.ItemType}'")
                            )
                        )
                    );
            }

            var result = await mediator.Send(
                new AddProjectItemCommand(
                    id,
                    request.UserId,
                    itemType,
                    request.ItemId,
                    request.PosX,
                    request.PosY,
                    request.PosZ
                ),
                cancellationToken);

            return result.Match(
                item => Results.Ok(item),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Adds an item to a project.")
        .WithDescription("Adds a plant, decoration, or soil item to a project canvas with position coordinates. User must be the owner.")
        .Accepts<AddProjectItemRequest>("application/json")
        .Produces<ProjectItemResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}