using Prickle.Application.Projects;
using Prickle.Application.Projects.GetAll;
using SharedKernel;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllProjectsRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public Guid? UserId { get; init; }
        public bool? IsPublished { get; init; }
    }

    public const string EndpointName = "GetAllProjects";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Projects.GetAll, async (
                [AsParameters] GetAllProjectsRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllProjectsQuery(request.UserId, request.IsPublished)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);
            return result.Match(
                projectsResponse => Results.Ok(projectsResponse),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithDescription("Retrieves all projects with optional filtering, sorting, and paging. " +
                         "Filter by userId and/or isPublished. " +
                         "Sort by 'createdat' or 'id'. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-createdat' for descending by creation date.")
        .WithSummary("Get all projects")
        .Produces<ProjectsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}