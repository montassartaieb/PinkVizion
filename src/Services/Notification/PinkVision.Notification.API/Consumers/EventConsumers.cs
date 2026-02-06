using MassTransit;
using PinkVision.Notification.API.DTOs;
using PinkVision.Notification.API.Services;

namespace PinkVision.Notification.API.Consumers;

// Consumer for appointment events
public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public AppointmentCreatedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<AppointmentCreatedEvent> context)
    {
        var evt = context.Message;
        
        // Notify patient
        await _notificationService.CreateAsync(new CreateNotificationRequest
        {
            UserId = evt.PatientId.ToString(),
            Type = "APPOINTMENT",
            Title = "Nouveau rendez-vous",
            Message = $"Votre rendez-vous est prévu le {evt.ScheduledAt:dd/MM/yyyy à HH:mm}",
            Data = new Dictionary<string, object>
            {
                { "appointmentId", evt.AppointmentId.ToString() },
                { "scheduledAt", evt.ScheduledAt }
            }
        });

        // Notify doctor
        await _notificationService.CreateAsync(new CreateNotificationRequest
        {
            UserId = evt.DoctorId.ToString(),
            Type = "APPOINTMENT",
            Title = "Nouveau rendez-vous patient",
            Message = $"Un nouveau rendez-vous est prévu le {evt.ScheduledAt:dd/MM/yyyy à HH:mm}",
            Data = new Dictionary<string, object>
            {
                { "appointmentId", evt.AppointmentId.ToString() },
                { "patientId", evt.PatientId.ToString() }
            }
        });
    }
}

// Consumer for diagnosis events
public class DiagnosisCompletedConsumer : IConsumer<DiagnosisCompletedEvent>
{
    private readonly INotificationService _notificationService;

    public DiagnosisCompletedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<DiagnosisCompletedEvent> context)
    {
        var evt = context.Message;
        
        await _notificationService.CreateAsync(new CreateNotificationRequest
        {
            UserId = evt.PatientId.ToString(),
            Type = "DIAGNOSIS",
            Title = "Résultat d'analyse disponible",
            Message = "Le résultat de votre mammographie est disponible",
            Data = new Dictionary<string, object>
            {
                { "diagnosisId", evt.DiagnosisId.ToString() },
                { "imageId", evt.ImageId.ToString() }
            }
        });
    }
}

// Event models
public record AppointmentCreatedEvent
{
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledAt { get; init; }
}

public record DiagnosisCompletedEvent
{
    public Guid DiagnosisId { get; init; }
    public Guid ImageId { get; init; }
    public Guid PatientId { get; init; }
    public string Label { get; init; } = string.Empty;
    public string RiskLevel { get; init; } = string.Empty;
}
