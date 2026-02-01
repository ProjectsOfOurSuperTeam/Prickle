
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Prickle.Api.Extensions;
using Prickle.Api.Infrastructure;
using Prickle.Application.Soil.Types;
using Prickle.Application.Soil.Types.Add;

namespace Prickle.Api.Endpoints.Soil;

internal sealed class Add : IEndpoint
{
    public sealed record AddSoilTypeRequest(string Name);
    public const string EndpointName = "AddSoilType";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Soil.Add, async (
            [FromBody] AddSoilTypeRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new AddSoilTypeCommand(request.Name), cancellationToken);

            return result.Match(
                   (soilType) => Results.CreatedAtRoute(Get.EndpointName, new { id = soilType.Id }, soilType),
                   CustomResults.Problem
               );
        })
        .WithName(EndpointName)
        .WithTags(Tags.Soil)
        .WithDescription("Adds a new soil type to the system.")
        .WithSummary("Add a new soil type")
        .Accepts<AddSoilTypeRequest>("application/json")
        .Produces<SoilTypeResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        //TODO
    }
}
