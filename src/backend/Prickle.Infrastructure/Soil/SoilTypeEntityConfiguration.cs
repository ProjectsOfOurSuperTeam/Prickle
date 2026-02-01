using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prickle.Domain.Soil;

namespace Prickle.Infrastructure.Soil;

internal sealed class SoilTypeEntityConfiguration : IEntityTypeConfiguration<SoilType>
{
    public void Configure(EntityTypeBuilder<SoilType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
    }
}
