using Prickle.Application.Projects.Delete;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Delete : IEndpoint
{
    public const string EndpointName = "DeleteProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Projects.Delete, async (
            [FromRoute] Guid id,
            [FromQuery] Guid userId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new DeleteProjectCommand(id, userId), cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Deletes a project by its ID.")
        .WithDescription("Deletes a project by its ID. User must be the owner. Returns 204 No Content if successful.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }
}