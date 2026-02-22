using Prickle.Application.Plants;
using Prickle.Application.Plants.GetAll;
using Prickle.Infrastructure.Authentication;
using SharedKernel;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllPlantsRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public string? Name { get; init; }
    }

    public const string EndpointName = "GetAllPlants";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Plants.GetAll, async (
                [AsParameters] GetAllPlantsRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllPlantsQuery(request.Name)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);
            return result.Match(
                plantsResponse => Results.Ok(plantsResponse),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithDescription("Retrieves all plants with optional filtering, sorting, and paging. " +
                         "Filter by name. " +
                         "Sort by 'name', 'namelatin' or 'id'. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-name' for descending by name, 'name' for ascending by name.")
        .WithSummary("Get all plants")
        .Produces<PlantsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}