using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Containers;

namespace Prickle.Infrastructure.Containers;

internal sealed class ContainersEntityConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Volume)
            .IsRequired();

        builder.Property(x => x.IsClosed);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.ImageIsometricUrl)
            .HasMaxLength(2048);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
