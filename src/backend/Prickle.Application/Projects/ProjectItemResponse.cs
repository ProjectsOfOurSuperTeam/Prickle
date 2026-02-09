using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Prickle.Domain.Projects;

namespace Prickle.Application.Projects;

public sealed record ProjectItemResponse
{
    public required Guid Id { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<ProjectItemType, int>))]
    public required ProjectItemType ItemType { get; init; }
    public required Guid ItemId { get; init; }
    public required int PosX { get; init; }
    public required int PosY { get; init; }
    public required int PosZ { get; init; }
}