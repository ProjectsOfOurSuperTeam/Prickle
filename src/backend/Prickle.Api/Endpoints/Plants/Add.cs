using Prickle.Application.Plants;
using Prickle.Application.Plants.Add;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;
using Prickle.Infrastructure.Authentication;
using SharedKernel;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class Add : IEndpoint
{
    public sealed record AddPlantRequest
    {
        public required string Name { get; init; } = string.Empty;
        public required string NameLatin { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
        public required int LightLevel { get; init; }
        public required int WaterNeed { get; init; }
        public required int HumidityLevel { get; init; }
        public required int ItemMaxSize { get; init; }
        public required Guid SoilFormulaId { get; init; }
    }
    public const string EndpointName = "AddPlant";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Plants.Add, async (
            [FromBody] AddPlantRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (!PlantLightLevel.TryFromValue(request.LightLevel, out var lightLevel))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            PlantErrors.InvalidLightLevel(request.LightLevel)
                            )
                        )
                    );
            }

            if (!PlantWaterNeed.TryFromValue(request.WaterNeed, out var waterNeed))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            PlantErrors.InvalidWaterNeed(request.WaterNeed)
                            )
                        )
                    );
            }

            if (!PlantHumidityLevel.TryFromValue(request.HumidityLevel, out var humidityLevel))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            PlantErrors.InvalidHumidityLevel(request.HumidityLevel)
                            )
                        )
                    );
            }

            if (!ProjectItemSize.TryFromValue(request.ItemMaxSize, out var itemMaxSize))
            {
                return await ValueTask.FromResult(
                    CustomResults.Problem(
                        Result.Failure(
                            PlantErrors.InvalidItemSize(request.ItemMaxSize)
                            )
                        )
                    );
            }

            var result = await mediator.Send(
                new AddPlantCommand(
                    request.Name,
                    request.NameLatin,
                    request.Description,
                    request.ImageUrl,
                    request.ImageIsometricUrl,
                    lightLevel,
                    waterNeed,
                    humidityLevel,
                    itemMaxSize,
                    request.SoilFormulaId
                ),
                cancellationToken);

            return result.Match(
                   (plant) => Results.CreatedAtRoute(Get.EndpointName, new { id = plant.Id }, plant),
                   CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Plants)
        .WithSummary("Adds a new plant.")
        .WithDescription("Adds a new plant.")
        .Accepts<AddPlantRequest>("application/json")
        .Produces<PlantResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.Admin);
    }
}