using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations;

public sealed record DecorationResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    [JsonConverter(typeof(SmartEnumNameConverter<DecorationCategory, int>))]
    public required DecorationCategory Category { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageIsometricUrl { get; init; }
}
