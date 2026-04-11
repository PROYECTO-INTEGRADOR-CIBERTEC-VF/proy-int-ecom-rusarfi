using Microsoft.EntityFrameworkCore;
using RusarfiServer.Models;

namespace RusarfiServer.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Name).HasMaxLength(120);
            entity.Property(u => u.Email).HasMaxLength(320);
            entity.Property(u => u.PasswordHash).HasMaxLength(255);
        });
    }
}
