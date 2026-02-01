
using Microsoft.AspNetCore.Mvc;

namespace Prickle.Api.Endpoints.Soil;

internal sealed class Get : IEndpoint
{
    public const string EndpointName = "GetSoilTypes";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Soil.Get, async ([FromRoute] int id, CancellationToken cancellationToken) =>
        {

        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil);
    }
}
