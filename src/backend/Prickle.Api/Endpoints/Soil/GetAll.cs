using Prickle.Application.Soil.Types;
using Prickle.Application.Soil.Types.GetAll;
using SharedKernel;

namespace Prickle.Api.Endpoints.Soil;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllSoilTypesRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public string? Name { get; init; }
    }
    public const string EndpointName = "GetAllSoilTypes";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.GetAll, async (
                [AsParameters] GetAllSoilTypesRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllSoilTypeQuery(request.Name)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);

            return result.Match(
                soilTypesResponse => Results.Ok(soilTypesResponse),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil)
        .WithDescription("Retrieves all soil types with optional filtering, sorting, and paging. " +
                         "Sort by any field using SortBy. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-name' for descending by name.")
        .WithSummary("Get all soil types.")
        .Produces<SoilTypesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        //TODO: validation
    }
}
