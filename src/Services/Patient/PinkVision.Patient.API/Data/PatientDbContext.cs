using Microsoft.EntityFrameworkCore;
using PinkVision.Patient.API.Entities;

namespace PinkVision.Patient.API.Data;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();
    public DbSet<BloodGroup> BloodGroups => Set<BloodGroup>();
    public DbSet<VitalsHistory> VitalsHistory => Set<VitalsHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient configuration
        modelBuilder.Entity<PatientEntity>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Email);

            entity.HasOne(p => p.BloodGroup)
                .WithMany()
                .HasForeignKey(p => p.BloodGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // VitalsHistory configuration
        modelBuilder.Entity<VitalsHistory>(entity =>
        {
            entity.HasIndex(e => new { e.PatientId, e.MeasuredAt });

            entity.HasOne(v => v.Patient)
                .WithMany(p => p.VitalsHistory)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed blood groups
        modelBuilder.Entity<BloodGroup>().HasData(
            new BloodGroup { Id = 1, Code = "A+", Description = "A Positif" },
            new BloodGroup { Id = 2, Code = "A-", Description = "A Négatif" },
            new BloodGroup { Id = 3, Code = "B+", Description = "B Positif" },
            new BloodGroup { Id = 4, Code = "B-", Description = "B Négatif" },
            new BloodGroup { Id = 5, Code = "AB+", Description = "AB Positif" },
            new BloodGroup { Id = 6, Code = "AB-", Description = "AB Négatif" },
            new BloodGroup { Id = 7, Code = "O+", Description = "O Positif" },
            new BloodGroup { Id = 8, Code = "O-", Description = "O Négatif" }
        );
    }
}
