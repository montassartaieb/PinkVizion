using Microsoft.EntityFrameworkCore;
using PinkVision.Doctor.API.Entities;

namespace PinkVision.Doctor.API.Data;

public class DoctorDbContext : DbContext
{
    public DoctorDbContext(DbContextOptions<DoctorDbContext> options) : base(options) { }

    public DbSet<DoctorEntity> Doctors => Set<DoctorEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DoctorEntity>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.LicenseNumber);
        });
    }
}
