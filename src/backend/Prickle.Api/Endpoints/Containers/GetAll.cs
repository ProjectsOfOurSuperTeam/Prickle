using Prickle.Application.Containers;
using Prickle.Application.Containers.GetAll;
using SharedKernel;

namespace Prickle.Api.Endpoints.Containers;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllContainersRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public string? Name { get; init; }
    }

    public const string EndpointName = "GetAllContainers";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Containers.GetAll, async (
                [AsParameters] GetAllContainersRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllContainersQuery(request.Name)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);
            return result.Match(
                containersResponse => Results.Ok(containersResponse),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Containers)
        .WithDescription("Retrieves all containers with optional filtering, sorting, and paging. " +
                         "Filter by name. " +
                         "Sort by 'name', 'id', or 'volume'. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-name' for descending by name, 'volume' for ascending by volume.")
        .WithSummary("Get all containers")
        .Produces<ContainersResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}