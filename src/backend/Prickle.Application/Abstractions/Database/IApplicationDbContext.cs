using Microsoft.EntityFrameworkCore;
using Prickle.Domain.Containers;
using Prickle.Domain.Decorations;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;
using Prickle.Domain.Soil;

namespace Prickle.Application.Abstractions.Database;

public interface IApplicationDbContext
{
    DbSet<SoilType> SoilTypes { get; }
    DbSet<SoilFormulas> SoilFormulas { get; }
    DbSet<SoilTypeSoilFormula> SoilTypeSoilFormulas { get; }
    DbSet<Decoration> Decorations { get; }
    DbSet<Container> Containers { get; }
    DbSet<Plant> Plants { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectItem> ProjectItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
