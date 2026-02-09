using Prickle.Application.Projects;
using Prickle.Application.Projects.Add;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Add : IEndpoint
{
    public sealed record AddProjectRequest
    {
        public required Guid UserId { get; init; }
        public required Guid ContainerId { get; init; }
        public byte[]? Preview { get; init; }
    }

    public const string EndpointName = "AddProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Add, async (
            [FromBody] AddProjectRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new AddProjectCommand(
                    request.UserId,
                    request.ContainerId,
                    request.Preview
                ),
                cancellationToken);

            return result.Match(
                   (project) => Results.CreatedAtRoute(Get.EndpointName, new { id = project.Id }, project),
                   CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Adds a new project.")
        .WithDescription("Creates a new project for the specified user and container.")
        .Accepts<AddProjectRequest>("application/json")
        .Produces<ProjectResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}