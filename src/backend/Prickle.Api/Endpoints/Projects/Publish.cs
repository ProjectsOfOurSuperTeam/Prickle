using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.Publish;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Publish : IEndpoint
{

    public const string EndpointName = "PublishProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.Publish, async (
            [FromRoute] Guid id,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(
                new PublishProjectCommand(id, userContext.UserId),
                cancellationToken);

            return result.Match(
                project => Results.Ok(project),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Publishes a project.")
        .WithDescription("Marks a project as published. User must be the owner.")
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}