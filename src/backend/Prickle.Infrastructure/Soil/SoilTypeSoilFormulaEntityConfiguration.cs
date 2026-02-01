using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Soil;

namespace Prickle.Infrastructure.Soil;

internal sealed class SoilTypeSoilFormulaEntityConfiguration : IEntityTypeConfiguration<SoilTypeSoilFormula>
{
    public void Configure(EntityTypeBuilder<SoilTypeSoilFormula> builder)
    {
        builder.HasKey(x => new { x.SoilFormulaId, x.SoilTypeId, x.Order });

        builder.Property(x => x.Percentage).IsRequired();
        builder.Property(x => x.Order).IsRequired();

        builder.HasOne<SoilType>()
                    .WithMany()
                    .HasForeignKey(x => x.SoilTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
    }
}