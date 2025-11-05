using Microsoft.EntityFrameworkCore;
using CrimsonBackend.Models;

namespace CrimsonBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<GameData> GameDatas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nickname).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Nickname);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // Configure GameData entity
        modelBuilder.Entity<GameData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Score).HasDefaultValue(0);
            entity.Property(e => e.Coin).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            // Configure one-to-one relationship
            entity.HasOne(gd => gd.Player)
                .WithOne(p => p.GameData)
                .HasForeignKey<GameData>(gd => gd.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Player || e.Entity is GameData);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Player player)
                {
                    player.CreatedAt = DateTime.UtcNow;
                    player.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is GameData gameData)
                {
                    gameData.CreatedAt = DateTime.UtcNow;
                    gameData.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Player player)
                {
                    player.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is GameData gameData)
                {
                    gameData.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

