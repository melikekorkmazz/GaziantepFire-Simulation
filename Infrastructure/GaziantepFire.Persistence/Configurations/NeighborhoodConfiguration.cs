using GaziantepFire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaziantepFire.Persistence.Configurations;

public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
{
    public void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Name).IsRequired().HasMaxLength(100);
        builder.Property(n => n.PolygonBoundaryGeoJson).IsRequired();
        builder.Property(n => n.RiskLevel).IsRequired();

        builder.HasOne(n => n.District)
               .WithMany(d => d.Neighborhoods)
               .HasForeignKey(n => n.DistrictId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
