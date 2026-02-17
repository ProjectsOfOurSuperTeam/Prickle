using Prickle.Application.Plants;
using Prickle.Application.Plants.Update;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;
using Prickle.Infrastructure.Authentication;
using SharedKernel;

namespace Prickle.Api.Endpoints.Plants;

internal sealed class Update : IEndpoint
{
    public sealed record UpdatePlantRequest
    {
        public required string Name { get; init; } = string.Empty;
        public required string NameLatin { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageIsometricUrl { get; init; }
        public required int Category { get; init; }
        public required int LightLevel { get; init; }
        public required int WaterNeed { get; init; }
        public required int HumidityLevel { get; init; }
        public required int ItemMaxSize { get; init; }
        public required Guid SoilFormulaId { get; init; }
    }
    public const string EndpointName = "UpdatePlant";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Plants.Update,
            async (
                [FromRoute] Guid id,
                UpdatePlantRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                if (!PlantCategory.TryFromValue(request.Category, out var category))
                {
                    return await ValueTask.FromResult(
                        CustomResults.Problem(
                            Result.Failure(
                                PlantErrors.InvalidCategory(request.Category)
                                )
                            )
                        );
                }

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

                var command = new UpdatePlantCommand(
                    id,
                    request.Name,
                    request.NameLatin,
                    request.Description,
                    request.ImageUrl,
                    request.ImageIsometricUrl,
                    category,
                    lightLevel,
                    waterNeed,
                    humidityLevel,
                    itemMaxSize,
                    request.SoilFormulaId);

                var result = await mediator.Send(command, cancellationToken);
                return result.Match(
                    plant => Results.Ok(plant),
                    CustomResults.Problem);
            })
            .WithName(EndpointName)
            .WithTags(Tags.Plants)
            .WithSummary("Updates a plant.")
            .WithDescription("Updates an existing plant with the provided details.")
            .Accepts<UpdatePlantRequest>("application/json")
            .Produces<PlantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .HasPermission(AuthorizationPolicies.Admin);
    }
}