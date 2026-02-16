using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.Add;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Add : IEndpoint
{
    public sealed record AddProjectRequest
    {
        public required Guid ContainerId { get; init; }
        public byte[]? Preview { get; init; }
    }

    public const string EndpointName = "AddProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Add, async (
            [FromBody] AddProjectRequest request,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new AddProjectCommand(
                    userContext.UserId,
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
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}