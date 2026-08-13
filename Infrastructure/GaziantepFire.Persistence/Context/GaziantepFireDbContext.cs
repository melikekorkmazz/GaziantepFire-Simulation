using GaziantepFire.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.Persistence.Context;

public class GaziantepFireDbContext : DbContext
{
    public GaziantepFireDbContext(DbContextOptions<GaziantepFireDbContext> options) : base(options)
    {
    }

    public DbSet<District> Districts => Set<District>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<FireStation> FireStations => Set<FireStation>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GaziantepFireDbContext).Assembly);
    }
}
