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
        });

        // Configure GameData entity
        modelBuilder.Entity<GameData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Score).HasDefaultValue(0);
            entity.Property(e => e.Coin).HasDefaultValue(0);
            
            // Configure one-to-one relationship
            entity.HasOne(gd => gd.Player)
                .WithOne(p => p.GameData)
                .HasForeignKey<GameData>(gd => gd.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

