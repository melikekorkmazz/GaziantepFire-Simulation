using GaziantepFire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaziantepFire.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ExternalId);  // nullable, API ihbar ID
        builder.Property(i => i.SubType).HasMaxLength(100);
        builder.Property(i => i.IncidentType).IsRequired();
        builder.Property(i => i.Latitude).IsRequired();
        builder.Property(i => i.Longitude).IsRequired();
        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.ResponseTimeInMinutes).IsRequired();

        builder.HasOne(i => i.Neighborhood)
               .WithMany(n => n.Incidents)
               .HasForeignKey(i => i.NeighborhoodId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
