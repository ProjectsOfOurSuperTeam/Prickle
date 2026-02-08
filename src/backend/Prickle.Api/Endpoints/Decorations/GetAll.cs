using Prickle.Application.Decorations;
using Prickle.Application.Decorations.GetAll;
using SharedKernel;

namespace Prickle.Api.Endpoints.Decorations;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllDecorationsRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public string? Name { get; init; }
    }

    public const string EndpointName = "GetAllDecorations";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Decorations.GetAll, async (
                [AsParameters] GetAllDecorationsRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllDecorationQuery(request.Name)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);
            return result.Match(
                decorationsResponse => Results.Ok(decorationsResponse),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Decorations)
        .WithDescription("Retrieves all decorations with optional filtering, sorting, and paging. " +
                         "Filter by name. " +
                         "Sort by 'name' or 'id'. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-name' for descending by name, 'name' for ascending by name.")
        .WithSummary("Get all decorations")
        .Produces<DecorationsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}