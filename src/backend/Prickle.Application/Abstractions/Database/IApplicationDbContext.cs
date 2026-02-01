using Microsoft.EntityFrameworkCore;
using Prickle.Domain.Soil;

namespace Prickle.Application.Abstractions.Database;

public interface IApplicationDbContext
{
    DbSet<SoilType> SoilTypes { get; }
    DbSet<SoilFormulas> SoilFormulas { get; }
    DbSet<SoilTypeSoilFormula> SoilTypeSoilFormulas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
