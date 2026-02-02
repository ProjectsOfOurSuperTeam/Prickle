
namespace Prickle.Api.Endpoints.Soil.Formulas;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetSoilFormula";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.Formulas.Get, ([FromRoute] Guid id) =>
        {
            return Results.Ok();
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil, Tags.SoilFormulas);
    }
}
