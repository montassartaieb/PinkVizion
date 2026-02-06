using Microsoft.EntityFrameworkCore;
using PinkVision.Appointment.API.Entities;

namespace PinkVision.Appointment.API.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

    public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppointmentEntity>(entity =>
        {
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.DoctorId);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.Status);
        });
    }
}
