namespace Prickle.Application.Soil.Types;

public sealed record SoilTypeResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
