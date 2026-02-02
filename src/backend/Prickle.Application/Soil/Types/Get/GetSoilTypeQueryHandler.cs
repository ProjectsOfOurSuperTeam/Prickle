using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Types.Get;

internal sealed class GetSoilTypeQueryHandler
    (IApplicationDbContext dbContext)
    : IQueryHandler<GetSoilTypeQuery, Result<SoilTypeResponse>>
{
    public async ValueTask<Result<SoilTypeResponse>> Handle(GetSoilTypeQuery query, CancellationToken cancellationToken)
    {
        var soilType = await dbContext.SoilTypes
            .AsNoTracking()
            .Where(st => st.Id == query.Id)
            .Select(st => new SoilTypeResponse(
                st.Id,
                st.Name))
            .FirstOrDefaultAsync(cancellationToken);
        return soilType is not null
            ? Result.Success(soilType)
            : Result.Failure<SoilTypeResponse>(SoilErrors.SoilType.NotFound(query.Id));
    }
}
