using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Projects;

namespace Prickle.Infrastructure.Projects;

internal sealed class ProjectItemEntityConfiguration : IEntityTypeConfiguration<ProjectItem>
{
    public void Configure(EntityTypeBuilder<ProjectItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.ItemType)
            .IsRequired();

        builder.Property(x => x.ItemId)
            .IsRequired();

        builder.Property(x => x.PosX)
            .IsRequired();

        builder.Property(x => x.PosY)
            .IsRequired();

        builder.Property(x => x.PosZ)
            .IsRequired();

        builder.HasIndex(x => x.ProjectId);

        builder.HasIndex(x => x.ItemType);
    }
}