using Microsoft.EntityFrameworkCore;
using PinkVision.Doctor.API.Data;
using PinkVision.Doctor.API.DTOs;
using PinkVision.Doctor.API.Entities;

namespace PinkVision.Doctor.API.Services;

public interface IDoctorService
{
    Task<ApiResponse<DoctorDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<DoctorDto>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<PagedResult<DoctorDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? specialty = null);
    Task<ApiResponse<DoctorDto>> CreateAsync(CreateDoctorRequest request);
    Task<ApiResponse<DoctorDto>> UpdateAsync(Guid id, UpdateDoctorRequest request);
    Task<ApiResponse<List<DoctorDto>>> GetAvailableDoctorsAsync();
}

public class DoctorService : IDoctorService
{
    private readonly DoctorDbContext _context;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(DoctorDbContext context, ILogger<DoctorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<DoctorDto>> GetByIdAsync(Guid id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
            return new ApiResponse<DoctorDto> { Success = false, Message = "Médecin non trouvé" };

        return new ApiResponse<DoctorDto> { Success = true, Data = MapToDto(doctor) };
    }

    public async Task<ApiResponse<DoctorDto>> GetByUserIdAsync(Guid userId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null)
            return new ApiResponse<DoctorDto> { Success = false, Message = "Profil médecin non trouvé" };

        return new ApiResponse<DoctorDto> { Success = true, Data = MapToDto(doctor) };
    }

    public async Task<ApiResponse<PagedResult<DoctorDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? specialty = null)
    {
        var query = _context.Doctors.Where(d => d.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(specialty))
            query = query.Where(d => d.Specialty != null && d.Specialty.Contains(specialty));

        var totalCount = await query.CountAsync();
        var doctors = await query
            .OrderBy(d => d.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ApiResponse<PagedResult<DoctorDto>>
        {
            Success = true,
            Data = new PagedResult<DoctorDto>
            {
                Items = doctors.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            }
        };
    }

    public async Task<ApiResponse<DoctorDto>> CreateAsync(CreateDoctorRequest request)
    {
        if (await _context.Doctors.AnyAsync(d => d.UserId == request.UserId))
            return new ApiResponse<DoctorDto> { Success = false, Message = "Profil médecin déjà existant" };

        var doctor = new DoctorEntity
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialty = request.Specialty,
            LicenseNumber = request.LicenseNumber,
            Hospital = request.Hospital,
            Department = request.Department
        };

        await _context.Doctors.AddAsync(doctor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Doctor created: {DoctorId}", doctor.Id);
        return new ApiResponse<DoctorDto> { Success = true, Message = "Profil créé", Data = MapToDto(doctor) };
    }

    public async Task<ApiResponse<DoctorDto>> UpdateAsync(Guid id, UpdateDoctorRequest request)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
            return new ApiResponse<DoctorDto> { Success = false, Message = "Médecin non trouvé" };

        if (request.FirstName != null) doctor.FirstName = request.FirstName;
        if (request.LastName != null) doctor.LastName = request.LastName;
        if (request.Phone != null) doctor.Phone = request.Phone;
        if (request.Specialty != null) doctor.Specialty = request.Specialty;
        if (request.Hospital != null) doctor.Hospital = request.Hospital;
        if (request.Department != null) doctor.Department = request.Department;
        if (request.Bio != null) doctor.Bio = request.Bio;
        if (request.IsAvailable.HasValue) doctor.IsAvailable = request.IsAvailable.Value;

        doctor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<DoctorDto> { Success = true, Message = "Profil mis à jour", Data = MapToDto(doctor) };
    }

    public async Task<ApiResponse<List<DoctorDto>>> GetAvailableDoctorsAsync()
    {
        var doctors = await _context.Doctors
            .Where(d => d.IsActive && d.IsAvailable)
            .OrderBy(d => d.LastName)
            .ToListAsync();

        return new ApiResponse<List<DoctorDto>>
        {
            Success = true,
            Data = doctors.Select(MapToDto).ToList()
        };
    }

    private static DoctorDto MapToDto(DoctorEntity doctor) => new()
    {
        Id = doctor.Id,
        UserId = doctor.UserId,
        FirstName = doctor.FirstName,
        LastName = doctor.LastName,
        Email = doctor.Email,
        Phone = doctor.Phone,
        Specialty = doctor.Specialty,
        LicenseNumber = doctor.LicenseNumber,
        Hospital = doctor.Hospital,
        Department = doctor.Department,
        Bio = doctor.Bio,
        IsAvailable = doctor.IsAvailable,
        IsActive = doctor.IsActive,
        CreatedAt = doctor.CreatedAt
    };
}
