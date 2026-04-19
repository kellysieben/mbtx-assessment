using Microsoft.EntityFrameworkCore;
using MbtxAssessment.DataStore;

namespace MbtxAssessment;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RegisteredClientEntity> RegisteredClients => Set<RegisteredClientEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredClientEntity>(entity =>
        {
            entity.HasKey(e => e.ClientId);
            entity.Property(e => e.ClientId)
                  .HasMaxLength(256)
                  .UseCollation("NOCASE");
        });
    }
}
