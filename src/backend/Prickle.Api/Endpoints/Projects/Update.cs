using Prickle.Application.Projects;
using Prickle.Application.Projects.Update;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateProjectRequest
    {
        public required Guid UserId { get; init; }
        public byte[]? Preview { get; init; }
    }

    public const string EndpointName = "UpdateProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Projects.Update,
            async (
                [FromRoute] Guid id,
                UpdateProjectRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProjectCommand(
                    id,
                    request.UserId,
                    request.Preview);

                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    project => Results.Ok(project),
                    CustomResults.Problem);
            })
            .WithName(EndpointName)
            .WithTags(Tags.Projects)
            .WithSummary("Updates a project.")
            .WithDescription("Updates an existing project's preview image.")
            .Accepts<UpdateProjectRequest>("application/json")
            .Produces<ProjectResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}