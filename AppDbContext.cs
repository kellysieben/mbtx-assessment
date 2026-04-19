using Microsoft.EntityFrameworkCore;
using MbtxAssessment.DataStore;

namespace MbtxAssessment;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RegisteredClientEntity> RegisteredClients => Set<RegisteredClientEntity>();
    public DbSet<SensorReadingEntity> SensorReadings => Set<SensorReadingEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredClientEntity>()
            .HasKey(c => c.ClientId);

        modelBuilder.Entity<SensorReadingEntity>()
            .HasKey(r => r.Id);
    }
}
