using Prickle.Domain.Projects;

namespace Prickle.Application.Projects;

internal static class ProjectMappings
{
    public static ProjectResponse ToResponse(this Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            UserId = project.UserId,
            ContainerId = project.ContainerId,
            Preview = project.Preview,
            GeneratedFlorariumImage = project.GeneratedFlorariumImage,
            GeneratedFlorariumImageMimeType = project.GeneratedFlorariumImageMimeType,
            FlorariumImageGenerationStatus = project.FlorariumImageGenerationStatus.ToString(),
            FlorariumImageGenerationError = project.FlorariumImageGenerationError,
            FlorariumImageRequestedAt = project.FlorariumImageRequestedAt,
            FlorariumImageCompletedAt = project.FlorariumImageCompletedAt,
            CreatedAt = project.CreatedAt,
            IsPublished = project.IsPublished,
            Items = project.Items.Select(item => new ProjectItemResponse
            {
                Id = item.Id,
                ItemType = item.ItemType,
                ItemId = item.ItemId,
                PosX = item.PosX,
                PosY = item.PosY,
                PosZ = item.PosZ
            }).ToList()
        };
    }
}