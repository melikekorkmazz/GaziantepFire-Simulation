using GaziantepFire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaziantepFire.Persistence.Configurations;

public class FireStationConfiguration : IEntityTypeConfiguration<FireStation>
{
    public void Configure(EntityTypeBuilder<FireStation> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(150);
        builder.Property(f => f.Latitude).IsRequired();
        builder.Property(f => f.Longitude).IsRequired();
        builder.Property(f => f.Capacity).IsRequired();
    }
}
