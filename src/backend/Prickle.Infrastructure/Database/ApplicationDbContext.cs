using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Containers;
using Prickle.Domain.Decorations;
using Prickle.Domain.Plants;
using Prickle.Domain.Projects;
using Prickle.Domain.Soil;
using SmartEnum.EFCore;

namespace Prickle.Infrastructure.Database;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<SoilType> SoilTypes { get; set; }
    public DbSet<SoilFormulas> SoilFormulas { get; set; }
    public DbSet<SoilTypeSoilFormula> SoilTypeSoilFormulas { get; set; }
    public DbSet<Decoration> Decorations { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectItem> ProjectItems { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureSmartEnum();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
