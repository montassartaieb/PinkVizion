using Microsoft.EntityFrameworkCore;
using PinkVision.MedicalRecord.API.Entities;

namespace PinkVision.MedicalRecord.API.Data;

public class MedicalRecordDbContext : DbContext
{
    public MedicalRecordDbContext(DbContextOptions<MedicalRecordDbContext> options) : base(options) { }

    public DbSet<MedicalRecordEntity> MedicalRecords => Set<MedicalRecordEntity>();
    public DbSet<MedicalEntry> MedicalEntries => Set<MedicalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MedicalRecordEntity>(entity =>
        {
            entity.HasIndex(e => e.PatientId).IsUnique();
        });

        modelBuilder.Entity<MedicalEntry>(entity =>
        {
            entity.HasOne(e => e.MedicalRecord)
                .WithMany(r => r.Entries)
                .HasForeignKey(e => e.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
