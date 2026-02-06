using Microsoft.EntityFrameworkCore;
using PinkVision.Appointment.API.Data;
using PinkVision.Appointment.API.DTOs;
using PinkVision.Appointment.API.Entities;
using MassTransit;

namespace PinkVision.Appointment.API.Services;

public interface IAppointmentService
{
    Task<ApiResponse<AppointmentDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<AppointmentDto>>> GetByPatientAsync(Guid patientId);
    Task<ApiResponse<List<AppointmentDto>>> GetByDoctorAsync(Guid doctorId, DateTime? date = null);
    Task<ApiResponse<AppointmentDto>> CreateAsync(CreateAppointmentRequest request);
    Task<ApiResponse<AppointmentDto>> ConfirmAsync(Guid id);
    Task<ApiResponse<AppointmentDto>> CancelAsync(Guid id, CancelAppointmentRequest request);
    Task<ApiResponse<AppointmentDto>> CompleteAsync(Guid id, string? notes = null);
    Task<ApiResponse<List<AppointmentDto>>> GetUpcomingAsync(Guid userId, bool isDoctor);
}

public class AppointmentService : IAppointmentService
{
    private readonly AppointmentDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(AppointmentDbContext context, IPublishEndpoint publishEndpoint, ILogger<AppointmentService> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentDto>> GetByIdAsync(Guid id)
    {
        var apt = await _context.Appointments.FindAsync(id);
        if (apt == null) return new ApiResponse<AppointmentDto> { Success = false, Message = "RDV non trouvé" };
        return new ApiResponse<AppointmentDto> { Success = true, Data = MapToDto(apt) };
    }

    public async Task<ApiResponse<List<AppointmentDto>>> GetByPatientAsync(Guid patientId)
    {
        var apts = await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();
        return new ApiResponse<List<AppointmentDto>> { Success = true, Data = apts.Select(MapToDto).ToList() };
    }

    public async Task<ApiResponse<List<AppointmentDto>>> GetByDoctorAsync(Guid doctorId, DateTime? date = null)
    {
        var query = _context.Appointments.Where(a => a.DoctorId == doctorId);
        
        if (date.HasValue)
            query = query.Where(a => a.ScheduledAt.Date == date.Value.Date);
        
        var apts = await query.OrderBy(a => a.ScheduledAt).ToListAsync();
        return new ApiResponse<List<AppointmentDto>> { Success = true, Data = apts.Select(MapToDto).ToList() };
    }

    public async Task<ApiResponse<AppointmentDto>> CreateAsync(CreateAppointmentRequest request)
    {
        // Check for conflicting appointments
        var conflict = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.Status != "CANCELLED" &&
            a.ScheduledAt < request.ScheduledAt.AddMinutes(request.DurationMinutes) &&
            request.ScheduledAt < a.ScheduledAt.AddMinutes(a.DurationMinutes));

        if (conflict)
            return new ApiResponse<AppointmentDto> { Success = false, Message = "Créneau non disponible" };

        var apt = new AppointmentEntity
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Status = "PENDING"
        };

        await _context.Appointments.AddAsync(apt);
        await _context.SaveChangesAsync();

        // Publish event
        await _publishEndpoint.Publish(new AppointmentCreatedEvent
        {
            AppointmentId = apt.Id,
            PatientId = apt.PatientId,
            DoctorId = apt.DoctorId,
            ScheduledAt = apt.ScheduledAt
        });

        _logger.LogInformation("Appointment created: {Id}", apt.Id);
        return new ApiResponse<AppointmentDto> { Success = true, Message = "RDV créé", Data = MapToDto(apt) };
    }

    public async Task<ApiResponse<AppointmentDto>> ConfirmAsync(Guid id)
    {
        var apt = await _context.Appointments.FindAsync(id);
        if (apt == null) return new ApiResponse<AppointmentDto> { Success = false, Message = "RDV non trouvé" };

        apt.Status = "CONFIRMED";
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<AppointmentDto> { Success = true, Message = "RDV confirmé", Data = MapToDto(apt) };
    }

    public async Task<ApiResponse<AppointmentDto>> CancelAsync(Guid id, CancelAppointmentRequest request)
    {
        var apt = await _context.Appointments.FindAsync(id);
        if (apt == null) return new ApiResponse<AppointmentDto> { Success = false, Message = "RDV non trouvé" };

        apt.Status = "CANCELLED";
        apt.CancelledAt = DateTime.UtcNow;
        apt.CancellationReason = request.CancellationReason;
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<AppointmentDto> { Success = true, Message = "RDV annulé", Data = MapToDto(apt) };
    }

    public async Task<ApiResponse<AppointmentDto>> CompleteAsync(Guid id, string? notes = null)
    {
        var apt = await _context.Appointments.FindAsync(id);
        if (apt == null) return new ApiResponse<AppointmentDto> { Success = false, Message = "RDV non trouvé" };

        apt.Status = "COMPLETED";
        apt.Notes = notes;
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<AppointmentDto> { Success = true, Message = "RDV terminé", Data = MapToDto(apt) };
    }

    public async Task<ApiResponse<List<AppointmentDto>>> GetUpcomingAsync(Guid userId, bool isDoctor)
    {
        var query = isDoctor
            ? _context.Appointments.Where(a => a.DoctorId == userId)
            : _context.Appointments.Where(a => a.PatientId == userId);

        var apts = await query
            .Where(a => a.ScheduledAt >= DateTime.UtcNow && a.Status != "CANCELLED")
            .OrderBy(a => a.ScheduledAt)
            .Take(10)
            .ToListAsync();

        return new ApiResponse<List<AppointmentDto>> { Success = true, Data = apts.Select(MapToDto).ToList() };
    }

    private static AppointmentDto MapToDto(AppointmentEntity apt) => new()
    {
        Id = apt.Id,
        PatientId = apt.PatientId,
        DoctorId = apt.DoctorId,
        ScheduledAt = apt.ScheduledAt,
        DurationMinutes = apt.DurationMinutes,
        Status = apt.Status,
        Reason = apt.Reason,
        Notes = apt.Notes,
        CreatedAt = apt.CreatedAt,
        CancelledAt = apt.CancelledAt,
        CancellationReason = apt.CancellationReason
    };
}

public record AppointmentCreatedEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledAt { get; init; }
}
