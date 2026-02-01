using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Soil;

namespace Prickle.Infrastructure.Soil;

internal sealed class SoilFormulasEntityConfiguration : IEntityTypeConfiguration<SoilFormulas>
{
    public void Configure(EntityTypeBuilder<SoilFormulas> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasMany(x => x.Formula)
                    .WithOne()
                    .HasForeignKey(x => x.SoilFormulaId)
                    .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(SoilFormulas.Formula));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
