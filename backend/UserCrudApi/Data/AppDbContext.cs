using Microsoft.EntityFrameworkCore;
using UserCrudApi.Models;

namespace UserCrudApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("user");

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
