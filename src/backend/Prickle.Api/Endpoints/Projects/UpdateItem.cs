using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.UpdateItem;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class UpdateItem : IEndpoint
{
    public sealed record UpdateProjectItemRequest
    {
        public required int PosX { get; init; }
        public required int PosY { get; init; }
        public required int PosZ { get; init; }
    }

    public const string EndpointName = "UpdateProjectItem";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Projects.UpdateItem, async (
            [FromRoute] Guid projectId,
            [FromRoute] Guid itemId,
            [FromBody] UpdateProjectItemRequest request,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new UpdateProjectItemCommand(
                    projectId,
                    userContext.UserId,
                    itemId,
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
        .WithSummary("Updates a project item's position.")
        .WithDescription("Updates the position coordinates of an item on the project canvas. User must be the owner.")
        .Accepts<UpdateProjectItemRequest>("application/json")
        .Produces<ProjectItemResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}