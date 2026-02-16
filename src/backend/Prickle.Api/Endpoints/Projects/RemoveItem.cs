using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects.RemoveItem;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class RemoveItem : IEndpoint
{
    public const string EndpointName = "RemoveProjectItem";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Projects.RemoveItem, async (
            [FromRoute] Guid projectId,
            [FromRoute] Guid itemId,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new RemoveProjectItemCommand(projectId, userContext.UserId, itemId),
                cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Removes an item from a project.")
        .WithDescription("Removes an item from a project canvas. User must be the owner. Returns 204 No Content if successful.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .HasPermission(AuthorizationPolicies.User);
    }
}