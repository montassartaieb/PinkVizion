using Microsoft.EntityFrameworkCore;
using PinkVision.Patient.API.Data;
using PinkVision.Patient.API.DTOs;
using PinkVision.Patient.API.Entities;

namespace PinkVision.Patient.API.Services;

public interface IPatientService
{
    Task<ApiResponse<PatientDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<PatientDto>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<PagedResult<PatientDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<PatientDto>> CreateAsync(CreatePatientRequest request);
    Task<ApiResponse<PatientDto>> UpdateAsync(Guid id, UpdatePatientRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<List<BloodGroupDto>>> GetBloodGroupsAsync();
    Task<ApiResponse<VitalsHistoryDto>> RecordVitalsAsync(Guid patientId, RecordVitalsRequest request, Guid? doctorId = null);
    Task<ApiResponse<List<VitalsHistoryDto>>> GetVitalsHistoryAsync(Guid patientId, int limit = 10);
}

public class PatientService : IPatientService
{
    private readonly PatientDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(PatientDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PatientDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.BloodGroup)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    Message = "Patient non trouvé"
                };
            }

            return new ApiResponse<PatientDto>
            {
                Success = true,
                Data = MapToDto(patient)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient {PatientId}", id);
            return new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Erreur lors de la récupération du patient"
            };
        }
    }

    public async Task<ApiResponse<PatientDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.BloodGroup)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    Message = "Patient non trouvé"
                };
            }

            return new ApiResponse<PatientDto>
            {
                Success = true,
                Data = MapToDto(patient)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by userId {UserId}", userId);
            return new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Erreur lors de la récupération du patient"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<PatientDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        try
        {
            var query = _context.Patients
                .Include(p => p.BloodGroup)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.FirstName != null && p.FirstName.ToLower().Contains(searchTerm)) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(searchTerm)) ||
                    (p.Email != null && p.Email.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var patients = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ApiResponse<PagedResult<PatientDto>>
            {
                Success = true,
                Data = new PagedResult<PatientDto>
                {
                    Items = patients.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return new ApiResponse<PagedResult<PatientDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération des patients"
            };
        }
    }

    public async Task<ApiResponse<PatientDto>> CreateAsync(CreatePatientRequest request)
    {
        try
        {
            // Check if patient already exists for this user
            if (await _context.Patients.AnyAsync(p => p.UserId == request.UserId))
            {
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    Message = "Un profil patient existe déjà pour cet utilisateur"
                };
            }

            var patient = new PatientEntity
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Age = request.Age ?? CalculateAge(request.DateOfBirth),
                Gender = request.Gender,
                BloodGroupId = request.BloodGroupId,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,
                Address = request.Address,
                City = request.City,
                DiseaseFollowup = request.DiseaseFollowup,
                Allergies = request.Allergies,
                MedicalHistory = request.MedicalHistory,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Reload with blood group
            await _context.Entry(patient).Reference(p => p.BloodGroup).LoadAsync();

            _logger.LogInformation("Patient created: {PatientId}", patient.Id);

            return new ApiResponse<PatientDto>
            {
                Success = true,
                Message = "Patient créé avec succès",
                Data = MapToDto(patient)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Erreur lors de la création du patient"
            };
        }
    }

    public async Task<ApiResponse<PatientDto>> UpdateAsync(Guid id, UpdatePatientRequest request)
    {
        try
        {
            var patient = await _context.Patients
                .Include(p => p.BloodGroup)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return new ApiResponse<PatientDto>
                {
                    Success = false,
                    Message = "Patient non trouvé"
                };
            }

            // Update fields if provided
            if (request.FirstName != null) patient.FirstName = request.FirstName;
            if (request.LastName != null) patient.LastName = request.LastName;
            if (request.Phone != null) patient.Phone = request.Phone;
            if (request.DateOfBirth.HasValue) patient.DateOfBirth = request.DateOfBirth;
            if (request.Age.HasValue) patient.Age = request.Age;
            if (request.Gender != null) patient.Gender = request.Gender;
            if (request.BloodGroupId.HasValue) patient.BloodGroupId = request.BloodGroupId;
            if (request.WeightKg.HasValue) patient.WeightKg = request.WeightKg;
            if (request.HeightCm.HasValue) patient.HeightCm = request.HeightCm;
            if (request.Address != null) patient.Address = request.Address;
            if (request.City != null) patient.City = request.City;
            if (request.EmergencyContactName != null) patient.EmergencyContactName = request.EmergencyContactName;
            if (request.EmergencyContactPhone != null) patient.EmergencyContactPhone = request.EmergencyContactPhone;
            if (request.DiseaseFollowup != null) patient.DiseaseFollowup = request.DiseaseFollowup;
            if (request.Allergies != null) patient.Allergies = request.Allergies;
            if (request.MedicalHistory != null) patient.MedicalHistory = request.MedicalHistory;

            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload blood group if changed
            if (request.BloodGroupId.HasValue)
            {
                await _context.Entry(patient).Reference(p => p.BloodGroup).LoadAsync();
            }

            return new ApiResponse<PatientDto>
            {
                Success = true,
                Message = "Patient mis à jour avec succès",
                Data = MapToDto(patient)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {PatientId}", id);
            return new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour du patient"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Patient non trouvé",
                    Data = false
                };
            }

            // Soft delete
            patient.IsActive = false;
            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Patient supprimé avec succès",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient {PatientId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression du patient",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<List<BloodGroupDto>>> GetBloodGroupsAsync()
    {
        try
        {
            var bloodGroups = await _context.BloodGroups.ToListAsync();
            return new ApiResponse<List<BloodGroupDto>>
            {
                Success = true,
                Data = bloodGroups.Select(bg => new BloodGroupDto
                {
                    Id = bg.Id,
                    Code = bg.Code,
                    Description = bg.Description
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blood groups");
            return new ApiResponse<List<BloodGroupDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération des groupes sanguins"
            };
        }
    }

    public async Task<ApiResponse<VitalsHistoryDto>> RecordVitalsAsync(Guid patientId, RecordVitalsRequest request, Guid? doctorId = null)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return new ApiResponse<VitalsHistoryDto>
                {
                    Success = false,
                    Message = "Patient non trouvé"
                };
            }

            var vitals = new VitalsHistory
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                MeasuredAt = DateTime.UtcNow,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                HeartRate = request.HeartRate,
                Temperature = request.Temperature,
                Notes = request.Notes,
                RecordedByDoctorId = doctorId
            };

            await _context.VitalsHistory.AddAsync(vitals);

            // Update current weight/height on patient
            if (request.WeightKg.HasValue) patient.WeightKg = request.WeightKg;
            if (request.HeightCm.HasValue) patient.HeightCm = request.HeightCm;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<VitalsHistoryDto>
            {
                Success = true,
                Message = "Mesures vitales enregistrées",
                Data = MapToVitalsDto(vitals)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording vitals for patient {PatientId}", patientId);
            return new ApiResponse<VitalsHistoryDto>
            {
                Success = false,
                Message = "Erreur lors de l'enregistrement des mesures"
            };
        }
    }

    public async Task<ApiResponse<List<VitalsHistoryDto>>> GetVitalsHistoryAsync(Guid patientId, int limit = 10)
    {
        try
        {
            var vitals = await _context.VitalsHistory
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.MeasuredAt)
                .Take(limit)
                .ToListAsync();

            return new ApiResponse<List<VitalsHistoryDto>>
            {
                Success = true,
                Data = vitals.Select(MapToVitalsDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vitals history for patient {PatientId}", patientId);
            return new ApiResponse<List<VitalsHistoryDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération de l'historique"
            };
        }
    }

    private static int? CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return null;
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static PatientDto MapToDto(PatientEntity patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            UserId = patient.UserId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.Age,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup != null ? new BloodGroupDto
            {
                Id = patient.BloodGroup.Id,
                Code = patient.BloodGroup.Code,
                Description = patient.BloodGroup.Description
            } : null,
            WeightKg = patient.WeightKg,
            HeightCm = patient.HeightCm,
            Address = patient.Address,
            City = patient.City,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            DiseaseFollowup = patient.DiseaseFollowup,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory,
            IsActive = patient.IsActive,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }

    private static VitalsHistoryDto MapToVitalsDto(VitalsHistory vitals)
    {
        return new VitalsHistoryDto
        {
            Id = vitals.Id,
            PatientId = vitals.PatientId,
            MeasuredAt = vitals.MeasuredAt,
            WeightKg = vitals.WeightKg,
            HeightCm = vitals.HeightCm,
            BloodPressureSystolic = vitals.BloodPressureSystolic,
            BloodPressureDiastolic = vitals.BloodPressureDiastolic,
            HeartRate = vitals.HeartRate,
            Temperature = vitals.Temperature,
            Notes = vitals.Notes
        };
    }
}
