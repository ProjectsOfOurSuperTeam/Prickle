using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Formulas.GetAll;
using SharedKernel;

namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class GetAll : IEndpoint
{
    public sealed record GetAllSoilFormulasRequest : PagedRequest
    {
        public string? SortBy { get; init; }
        public string? Name { get; init; }
        public int[]? SoilTypeIds { get; init; }
    }

    public const string EndpointName = "GetAllSoilFormulas";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.Formulas.GetAll, async (
                [AsParameters] GetAllSoilFormulasRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
        {
            var query = new GetAllSoilFormulasQuery(request.Name, request.SoilTypeIds)
            {
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page ?? PagedRequest.DefaultPage,
                PageSize = request.PageSize ?? PagedRequest.DefaultPageSize
            };

            var result = await mediator.Send(query, cancellationToken);

            return result.Match(
                soilFormulasResponse => Results.Ok(soilFormulasResponse),
                CustomResults.Problem
            );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil, Tags.SoilFormulas)
        .WithDescription("Retrieves all soil formulas with optional filtering, sorting, and paging. " +
                         "Filter by name or soil type IDs (formulas must contain ALL specified soil types). " +
                         "Sort by 'name', 'id', or 'itemcount'. Prefix with '-' for descending, '+' or no prefix for ascending. " +
                         "Example: '-name' for descending by name, 'itemcount' for ascending by number of items.")
        .WithSummary("Get all soil formulas")
        .Produces<SoilFormulasResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
