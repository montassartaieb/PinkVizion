using Microsoft.EntityFrameworkCore;
using PinkVision.MedicalRecord.API.Data;
using PinkVision.MedicalRecord.API.DTOs;
using PinkVision.MedicalRecord.API.Entities;

namespace PinkVision.MedicalRecord.API.Services;

public interface IMedicalRecordService
{
    Task<ApiResponse<MedicalRecordDto>> GetByPatientIdAsync(Guid patientId);
    Task<ApiResponse<MedicalRecordDto>> CreateOrUpdateAsync(Guid patientId, CreateOrUpdateRecordRequest request);
    Task<ApiResponse<MedicalEntryDto>> AddEntryAsync(Guid patientId, AddEntryRequest request);
}

public class MedicalRecordService : IMedicalRecordService
{
    private readonly MedicalRecordDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(MedicalRecordDbContext context, ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<MedicalRecordDto>> GetByPatientIdAsync(Guid patientId)
    {
        var record = await _context.MedicalRecords
            .Include(r => r.Entries.OrderByDescending(e => e.Date))
            .FirstOrDefaultAsync(r => r.PatientId == patientId);

        if (record == null)
            return new ApiResponse<MedicalRecordDto> { Success = false, Message = "Dossier médical non trouvé" };

        return new ApiResponse<MedicalRecordDto> { Success = true, Data = MapToDto(record) };
    }

    public async Task<ApiResponse<MedicalRecordDto>> CreateOrUpdateAsync(Guid patientId, CreateOrUpdateRecordRequest request)
    {
        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.PatientId == patientId);

        if (record == null)
        {
            record = new MedicalRecordEntity { PatientId = patientId };
            await _context.MedicalRecords.AddAsync(record);
        }

        if (request.Allergies != null) record.Allergies = request.Allergies;
        if (request.ChronicConditions != null) record.ChronicConditions = request.ChronicConditions;
        if (request.Treatments != null) record.Treatments = request.Treatments;
        if (request.FamilyHistory != null) record.FamilyHistory = request.FamilyHistory;
        if (request.BloodType != null) record.BloodType = request.BloodType;

        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<MedicalRecordDto> { Success = true, Message = "Dossier mis à jour", Data = MapToDto(record) };
    }

    public async Task<ApiResponse<MedicalEntryDto>> AddEntryAsync(Guid patientId, AddEntryRequest request)
    {
        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.PatientId == patientId);
        if (record == null)
        {
            // Auto-create record if not exists
            record = new MedicalRecordEntity { PatientId = patientId };
            await _context.MedicalRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        var entry = new MedicalEntry
        {
            MedicalRecordId = record.Id,
            DoctorId = request.DoctorId,
            Type = request.Type,
            Description = request.Description,
            AttachmentUrls = request.AttachmentUrls ?? new List<string>()
        };

        await _context.MedicalEntries.AddAsync(entry);
        await _context.SaveChangesAsync();

        return new ApiResponse<MedicalEntryDto>
        {
            Success = true,
            Message = "Entrée ajoutée",
            Data = new MedicalEntryDto
            {
                Id = entry.Id,
                DoctorId = entry.DoctorId,
                Date = entry.Date,
                Type = entry.Type,
                Description = entry.Description,
                AttachmentUrls = entry.AttachmentUrls
            }
        };
    }

    private static MedicalRecordDto MapToDto(MedicalRecordEntity record) => new()
    {
        Id = record.Id,
        PatientId = record.PatientId,
        Allergies = record.Allergies,
        ChronicConditions = record.ChronicConditions,
        Treatments = record.Treatments,
        FamilyHistory = record.FamilyHistory,
        BloodType = record.BloodType,
        UpdatedAt = record.UpdatedAt,
        Entries = record.Entries?.Select(e => new MedicalEntryDto
        {
            Id = e.Id,
            DoctorId = e.DoctorId,
            Date = e.Date,
            Type = e.Type,
            Description = e.Description,
            AttachmentUrls = e.AttachmentUrls
        }).ToList() ?? new List<MedicalEntryDto>()
    };
}
