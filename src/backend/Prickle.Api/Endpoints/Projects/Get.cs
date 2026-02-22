using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects;
using Prickle.Application.Projects.Get;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetProject";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Projects.Get, async (
            [FromRoute] Guid id,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProjectQuery(id, userContext.UserId), cancellationToken);
            return result.Match(
                project => Results.Ok(project),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithDescription("Retrieves a project by its ID with all items.")
        .WithSummary("Get a project by ID")
        .Produces<ProjectResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}