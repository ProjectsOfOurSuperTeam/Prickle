using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Application.Abstractions.ImageGeneration;
using Prickle.Domain.Projects;
using Prickle.Domain.Soil;

namespace Prickle.Application.Projects.GenerateFlorariumImage;

internal sealed class GenerateFlorariumImageCommandHandler(
    IApplicationDbContext dbContext,
    IFlorariumImageGenerator imageGenerator)
    : ICommandHandler<GenerateFlorariumImageCommand, Result<GenerateFlorariumImageResponse>>
{
    public async ValueTask<Result<GenerateFlorariumImageResponse>> Handle(
        GenerateFlorariumImageCommand command,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .Include(p => p.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<GenerateFlorariumImageResponse>(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.UserId != command.UserId)
        {
            return Result.Failure<GenerateFlorariumImageResponse>(ProjectErrors.UserNotOwner(command.UserId));
        }

        var container = await dbContext.Containers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == project.ContainerId, cancellationToken);

        if (container is null)
        {
            return Result.Failure<GenerateFlorariumImageResponse>(Error.Problem(
                "GenerateFlorarium.ContainerNotFound",
                $"Container with ID '{project.ContainerId}' was not found."));
        }

        var plantItems = project.Items.Where(i => i.ItemType == ProjectItemType.Plant).ToList();
        var decorationItems = project.Items.Where(i => i.ItemType == ProjectItemType.Decoration).ToList();
        var soilItems = project.Items.Where(i => i.ItemType == ProjectItemType.Soil).ToList();

        var plantIds = plantItems.Select(i => i.ItemId).Distinct().ToList();
        var decorationIds = decorationItems.Select(i => i.ItemId).Distinct().ToList();
        var soilFormulaIds = soilItems.Select(i => i.ItemId).Distinct().ToList();

        var plants = await dbContext.Plants
            .AsNoTracking()
            .Where(p => plantIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var decorations = await dbContext.Decorations
            .AsNoTracking()
            .Where(d => decorationIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        SoilFormulas? soilFormula = null;
        if (soilFormulaIds.Count > 0)
        {
            soilFormula = await dbContext.SoilFormulas
                .Include(sf => sf.Formula)
                .AsNoTracking()
                .FirstOrDefaultAsync(sf => soilFormulaIds.Contains(sf.Id), cancellationToken);
        }

        var soilTypeIds = soilFormula?.Formula
            .Select(f => f.SoilTypeId)
            .Distinct()
            .ToList() ?? [];

        var soilTypes = soilTypeIds.Count > 0
            ? await dbContext.SoilTypes
                .AsNoTracking()
                .Where(st => soilTypeIds.Contains(st.Id))
                .ToDictionaryAsync(st => st.Id, cancellationToken)
            : new Dictionary<int, Prickle.Domain.Soil.SoilType>();

        var prompt = BuildPrompt(container, plantItems, plants, decorationItems, decorations, soilFormula, soilTypes);

        var generateResult = await imageGenerator.GenerateFlorariumImageAsync(
            prompt,
            command.AtlasImage,
            command.LayoutImage,
            command.ImageMimeType,
            cancellationToken);

        if (generateResult.IsFailure)
        {
            return Result.Failure<GenerateFlorariumImageResponse>(generateResult.Error);
        }

        return Result.Success(new GenerateFlorariumImageResponse(generateResult.Value, "image/png"));
    }

    private static string BuildPrompt(
        Prickle.Domain.Containers.Container container,
        List<ProjectItem> plantItems,
        Dictionary<Guid, Prickle.Domain.Plants.Plant> plants,
        List<ProjectItem> decorationItems,
        Dictionary<Guid, Prickle.Domain.Decorations.Decoration> decorations,
        SoilFormulas? soilFormula,
        Dictionary<int, Prickle.Domain.Soil.SoilType> soilTypes)
    {
        var plantsList = plantItems
            .Select(pi => plants.TryGetValue(pi.ItemId, out var plant) ? plant : null)
            .Where(p => p is not null)
            .Select(p => $"- {p!.NameLatin} (Local name: {p.Name}). Size scale: {p.ItemMaxSize.Name}. Description: {p.Description ?? "N/A"}")
            .ToList();

        var decorationsList = decorationItems
            .Select(di => decorations.TryGetValue(di.ItemId, out var dec) ? dec : null)
            .Where(d => d is not null)
            .Select(d => $"- {d!.Name}: {d.Description ?? "N/A"}")
            .ToList();

        var soilList = new List<string>();
        if (soilFormula is not null)
        {
            var orderedFormula = soilFormula.Formula.OrderBy(f => f.Order).ToList();
            foreach (var item in orderedFormula)
            {
                var soilTypeName = soilTypes.TryGetValue(item.SoilTypeId, out var st) ? st.Name : "Unknown";
                soilList.Add($"- Layer {item.Order}: {soilTypeName} (Takes up {item.Percentage}% of the soil volume).");
            }
        }

        return $"""
            Ultra-realistic, professional macro studio photography of a glass terrarium (florarium).
            Style: Cozy warm studio lighting, 8k resolution, highly detailed, photorealistic, cinematic lighting, soft shadows.
            Camera parameters: Frontal view angled 30 degrees downwards, aspect ratio 16:9.

            The spatial arrangement of the objects MUST strictly follow the provided 2.5D layout image. Use the provided atlas image as a visual reference for the exact textures, colors, and appearance of the specific items.

            Container details:
            Shape: {container.Name}
            Volume: {container.Volume} liters.
            Ensure the glass has realistic reflections, refractions, and specular highlights.

            Plants included (arranged per the layout):
            {string.Join("\n", plantsList)}

            Decorations included:
            {string.Join("\n", decorationsList)}

            Soil stratification (clearly visible through the transparent glass):
            The soil consists of distinct layers. Total mixture:
            {string.Join("\n", soilList)}

            Render the soil layers realistically, showing the distinct textures of sand, stones, or soil.
            """;
    }
}
