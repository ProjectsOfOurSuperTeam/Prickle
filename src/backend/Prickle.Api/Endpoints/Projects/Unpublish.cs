using Prickle.Application.Projects;
using Prickle.Application.Projects.Unpublish;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Unpublish : IEndpoint
{
    public sealed record UnpublishProjectRequest
    {
        public required Guid UserId { get; init; }
    }

    public const string EndpointName = "UnpublishProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Unpublish, async (
            [FromRoute] Guid id,
            [FromBody] UnpublishProjectRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new UnpublishProjectCommand(id, request.UserId),
                cancellationToken);

            return result.Match(
                project => Results.Ok(project),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Unpublishes a project.")
        .WithDescription("Marks a project as not published. User must be the owner.")
        .Accepts<UnpublishProjectRequest>("application/json")
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}