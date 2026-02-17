using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Decorations;

namespace Prickle.Infrastructure.Decorations;

internal sealed class DecorationEntityConfiguration : IEntityTypeConfiguration<Decoration>
{
    public void Configure(EntityTypeBuilder<Decoration> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.ImageIsometricUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.Category)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.Category);
    }
}