using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Containers;
using Prickle.Domain.Projects;

namespace Prickle.Infrastructure.Projects;

internal sealed class ProjectEntityConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ContainerId)
            .IsRequired();

        builder.Property(x => x.Preview)
            .HasColumnType("BYTEA");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.ContainerId);

        builder.HasIndex(x => x.CreatedAt)
            .IsDescending();

        builder.HasOne<Container>()
            .WithMany()
            .HasForeignKey(x => x.ContainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .EnableLazyLoading(false);
    }
}