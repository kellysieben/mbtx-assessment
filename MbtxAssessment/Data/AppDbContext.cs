using Microsoft.EntityFrameworkCore;

namespace MbtxAssessment.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SensorReadingEntity> SensorReadings => Set<SensorReadingEntity>();
    public DbSet<AnomalyEntity> Anomalies => Set<AnomalyEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReadingEntity>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<AnomalyEntity>()
            .HasKey(a => a.Id);
    }
}
