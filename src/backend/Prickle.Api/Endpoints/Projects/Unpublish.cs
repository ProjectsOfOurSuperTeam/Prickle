using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.Unpublish;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Unpublish : IEndpoint
{

    public const string EndpointName = "UnpublishProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Unpublish, async (
            [FromRoute] Guid id,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new UnpublishProjectCommand(id, userContext.UserId),
                cancellationToken);

            return result.Match(
                project => Results.Ok(project),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Unpublishes a project.")
        .WithDescription("Marks a project as not published. User must be the owner.")
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}