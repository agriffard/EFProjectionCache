using Microsoft.EntityFrameworkCore;
using SampleApp.Entities;

namespace SampleApp.Data;

/// <summary>
/// Sample application DbContext.
/// </summary>
public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId);
        });
    }
}
