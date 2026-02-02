
namespace Prickle.Application.Soil.Types.Get;

public sealed record GetSoilTypeQuery(int Id) : IQuery<Result<SoilTypeResponse>>;