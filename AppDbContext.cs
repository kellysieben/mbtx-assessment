using Microsoft.EntityFrameworkCore;

namespace MbtxAssessment;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SensorReadingEntity> SensorReadings => Set<SensorReadingEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReadingEntity>()
            .HasKey(r => r.Id);
    }
}
