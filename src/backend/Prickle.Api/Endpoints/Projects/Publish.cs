using Prickle.Application.Projects;
using Prickle.Application.Projects.Publish;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Publish : IEndpoint
{
    public sealed record PublishProjectRequest
    {
        public required Guid UserId { get; init; }
    }

    public const string EndpointName = "PublishProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Publish, async (
            [FromRoute] Guid id,
            [FromBody] PublishProjectRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new PublishProjectCommand(id, request.UserId),
                cancellationToken);

            return result.Match(
                project => Results.Ok(project),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Publishes a project.")
        .WithDescription("Marks a project as published. User must be the owner.")
        .Accepts<PublishProjectRequest>("application/json")
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}