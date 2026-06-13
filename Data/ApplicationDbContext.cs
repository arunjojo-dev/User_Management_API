using Microsoft.EntityFrameworkCore;
using User_Management_API.Models;

namespace User_Management_API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
