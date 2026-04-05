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

        builder.Property(x => x.GeneratedFlorariumImage)
            .HasColumnType("BYTEA");

        builder.Property(x => x.GeneratedFlorariumImageMimeType)
            .HasMaxLength(64);

        builder.Property(x => x.PendingFlorariumCanvasImage)
            .HasColumnType("BYTEA");

        builder.Property(x => x.PendingFlorariumCanvasImageMimeType)
            .HasMaxLength(64);

        builder.Property(x => x.FlorariumImageGenerationStatus)
            .IsRequired()
            .HasDefaultValue(FlorariumImageGenerationStatus.NotRequested);

        builder.Property(x => x.FlorariumImageRequestedAt);

        builder.Property(x => x.FlorariumImageCompletedAt);

        builder.Property(x => x.FlorariumImageGenerationError);

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

        builder.HasIndex(x => x.FlorariumImageGenerationStatus);

        builder.HasOne<Container>()
            .WithMany()
            .HasForeignKey(x => x.ContainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Project.Items))?.SetField("_items");

        builder.Navigation(x => x.Items)
            .EnableLazyLoading(false);
    }
}