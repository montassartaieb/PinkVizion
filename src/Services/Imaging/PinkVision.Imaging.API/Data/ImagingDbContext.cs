using Microsoft.EntityFrameworkCore;
using PinkVision.Imaging.API.Entities;

namespace PinkVision.Imaging.API.Data;

public class ImagingDbContext : DbContext
{
    public ImagingDbContext(DbContextOptions<ImagingDbContext> options) : base(options)
    {
    }

    public DbSet<MammographyImage> MammographyImages => Set<MammographyImage>();
    public DbSet<DiagnosisResult> DiagnosisResults => Set<DiagnosisResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MammographyImage configuration
        modelBuilder.Entity<MammographyImage>(entity =>
        {
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.UploadedAt);
            entity.HasIndex(e => e.Status);

            entity.HasOne(m => m.DiagnosisResult)
                .WithOne(d => d.Image)
                .HasForeignKey<DiagnosisResult>(d => d.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiagnosisResult configuration
        modelBuilder.Entity<DiagnosisResult>(entity =>
        {
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.DoctorValidated);
        });
    }
}
