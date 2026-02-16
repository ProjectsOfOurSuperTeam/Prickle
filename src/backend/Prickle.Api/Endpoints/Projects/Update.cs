using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.Update;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateProjectRequest
    {
        public byte[]? Preview { get; init; }
    }

    public const string EndpointName = "UpdateProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Projects.Update,
            async (
                [FromRoute] Guid id,
                [FromBody] UpdateProjectRequest request,
                IUserContext userContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProjectCommand(
                    id,
                    userContext.UserId,
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .HasPermission(AuthorizationPolicies.User);
    }
}