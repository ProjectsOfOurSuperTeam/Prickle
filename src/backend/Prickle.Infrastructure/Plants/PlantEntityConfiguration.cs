using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Plants;
using Prickle.Domain.Soil;

namespace Prickle.Infrastructure.Plants;

internal sealed class PlantEntityConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NameLatin)
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

        builder.Property(x => x.LightLevel)
            .IsRequired();

        builder.Property(x => x.WaterNeed)
            .IsRequired();

        builder.Property(x => x.HumidityLevel)
            .IsRequired();

        builder.Property(x => x.ItemMaxSize)
            .IsRequired();

        builder.Property(x => x.SoilFormulaId)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.NameLatin);

        builder.HasOne<SoilFormulas>()
            .WithMany()
            .HasForeignKey(x => x.SoilFormulaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}