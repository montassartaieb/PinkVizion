using Microsoft.EntityFrameworkCore;
using PinkVision.Imaging.API.Data;
using PinkVision.Imaging.API.DTOs;
using PinkVision.Imaging.API.Entities;
using MassTransit;

namespace PinkVision.Imaging.API.Services;

public interface IImagingService
{
    Task<ApiResponse<MammographyImageDto>> UploadImageAsync(UploadImageRequest request, Guid uploadedByUserId);
    Task<ApiResponse<DiagnosisResultDto>> AnalyzeImageAsync(Guid imageId, PatientFeaturesDto? features = null);
    Task<ApiResponse<MammographyImageDto>> GetImageByIdAsync(Guid id);
    Task<ApiResponse<List<MammographyImageDto>>> GetImagesByPatientAsync(Guid patientId);
    Task<ApiResponse<DiagnosisResultDto>> ValidateDiagnosisAsync(ValidateDiagnosisRequest request, Guid doctorId);
    Task<ApiResponse<AIServiceStatusDto>> GetAIServiceStatusAsync();
    Task<ApiResponse<List<DiagnosisResultDto>>> GetPendingValidationsAsync();
    Task<ApiResponse<bool>> DeleteImageAsync(Guid id);
}

public class ImagingService : IImagingService
{
    private readonly ImagingDbContext _context;
    private readonly IAIService _aiService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImagingService> _logger;
    private readonly string _uploadPath;

    public ImagingService(
        ImagingDbContext context,
        IAIService aiService,
        IPublishEndpoint publishEndpoint,
        IWebHostEnvironment environment,
        ILogger<ImagingService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _aiService = aiService;
        _publishEndpoint = publishEndpoint;
        _environment = environment;
        _logger = logger;
        _uploadPath = configuration["UploadPath"] ?? Path.Combine(environment.ContentRootPath, "uploads");
        
        // Ensure upload directory exists
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<ApiResponse<MammographyImageDto>> UploadImageAsync(UploadImageRequest request, Guid uploadedByUserId)
    {
        try
        {
            // Validate file type
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".dcm" };
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedTypes.Contains(extension))
            {
                return new ApiResponse<MammographyImageDto>
                {
                    Success = false,
                    Message = "Type de fichier non supporté. Formats acceptés: JPG, PNG, DICOM"
                };
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var patientFolder = Path.Combine(_uploadPath, request.PatientId.ToString());
            Directory.CreateDirectory(patientFolder);
            var filePath = Path.Combine(patientFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // Create database record
            var image = new MammographyImage
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                UploadedByUserId = uploadedByUserId,
                FileName = fileName,
                OriginalFileName = request.File.FileName,
                FilePath = filePath,
                FileSizeBytes = request.File.Length,
                ContentType = request.File.ContentType,
                ImageType = request.ImageType,
                ViewPosition = request.ViewPosition,
                Status = "PENDING",
                Notes = request.Notes,
                UploadedAt = DateTime.UtcNow
            };

            await _context.MammographyImages.AddAsync(image);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image uploaded: {ImageId} for patient {PatientId}", image.Id, request.PatientId);

            // Publish event
            await _publishEndpoint.Publish(new ImageUploadedEvent
            {
                ImageId = image.Id,
                PatientId = request.PatientId,
                UploadedAt = image.UploadedAt
            });

            return new ApiResponse<MammographyImageDto>
            {
                Success = true,
                Message = "Image uploadée avec succès",
                Data = MapToDto(image)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return new ApiResponse<MammographyImageDto>
            {
                Success = false,
                Message = "Erreur lors de l'upload de l'image"
            };
        }
    }

    public async Task<ApiResponse<DiagnosisResultDto>> AnalyzeImageAsync(Guid imageId, PatientFeaturesDto? features = null)
    {
        try
        {
            var image = await _context.MammographyImages
                .Include(m => m.DiagnosisResult)
                .FirstOrDefaultAsync(m => m.Id == imageId);

            if (image == null)
            {
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = false,
                    Message = "Image non trouvée"
                };
            }

            if (image.DiagnosisResult != null)
            {
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = true,
                    Message = "L'image a déjà été analysée",
                    Data = MapToDiagnosisDto(image.DiagnosisResult)
                };
            }

            // Update status
            image.Status = "ANALYZING";
            await _context.SaveChangesAsync();

            // Prepare features for AI
            var aiFeatures = new Dictionary<string, object>();
            if (features != null)
            {
                if (features.Age.HasValue) aiFeatures["age"] = features.Age.Value;
                if (!string.IsNullOrEmpty(features.Menopause)) aiFeatures["menopause"] = features.Menopause;
                if (features.TumorSize.HasValue) aiFeatures["tumor_size"] = features.TumorSize.Value;
                if (!string.IsNullOrEmpty(features.InvNodes)) aiFeatures["inv_nodes"] = features.InvNodes;
                if (features.NodeCaps.HasValue) aiFeatures["node_caps"] = features.NodeCaps.Value;
                if (features.DegMalig.HasValue) aiFeatures["deg_malig"] = features.DegMalig.Value;
                if (!string.IsNullOrEmpty(features.Breast)) aiFeatures["breast"] = features.Breast;
                if (!string.IsNullOrEmpty(features.BreastQuad)) aiFeatures["breast_quad"] = features.BreastQuad;
                if (!string.IsNullOrEmpty(features.Irradiat)) aiFeatures["irradiat"] = features.Irradiat;
            }

            // Call AI service
            AIPredictResponse? aiResponse;
            try
            {
                using var fileStream = new FileStream(image.FilePath!, FileMode.Open, FileAccess.Read);
                aiResponse = await _aiService.PredictAsync(fileStream, image.OriginalFileName, aiFeatures);
            }
            catch (TimeoutException)
            {
                image.Status = "FAILED";
                await _context.SaveChangesAsync();
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = false,
                    Message = "Le service IA n'a pas répondu dans le délai imparti"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI service error for image {ImageId}", imageId);
                image.Status = "FAILED";
                await _context.SaveChangesAsync();
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = false,
                    Message = "Erreur lors de l'analyse IA"
                };
            }

            if (aiResponse == null)
            {
                image.Status = "FAILED";
                await _context.SaveChangesAsync();
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = false,
                    Message = "Le service IA n'a pas pu analyser l'image"
                };
            }

            // Create diagnosis result
            var diagnosis = new DiagnosisResult
            {
                Id = Guid.NewGuid(),
                ImageId = imageId,
                PatientId = image.PatientId,
                Label = aiResponse.Label,
                Probability = aiResponse.Probability,
                PImage = aiResponse.PImage,
                PTabular = aiResponse.PTabular,
                ModelVersion = aiResponse.ModelVersion,
                DegradedMode = aiResponse.DegradedMode,
                DoctorValidated = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.DiagnosisResults.AddAsync(diagnosis);
            
            image.Status = "ANALYZED";
            image.AnalyzedAt = DateTime.UtcNow;
            image.DiagnosisResult = diagnosis;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image analyzed: {ImageId}, Result: {Label} ({Probability:P})", 
                imageId, aiResponse.Label, aiResponse.Probability);

            // Publish event
            await _publishEndpoint.Publish(new DiagnosisCompletedEvent
            {
                DiagnosisId = diagnosis.Id,
                ImageId = imageId,
                PatientId = image.PatientId,
                Label = diagnosis.Label,
                Probability = diagnosis.Probability,
                CompletedAt = DateTime.UtcNow
            });

            return new ApiResponse<DiagnosisResultDto>
            {
                Success = true,
                Message = "Analyse terminée avec succès",
                Data = MapToDiagnosisDto(diagnosis)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image {ImageId}", imageId);
            return new ApiResponse<DiagnosisResultDto>
            {
                Success = false,
                Message = "Erreur lors de l'analyse"
            };
        }
    }

    public async Task<ApiResponse<MammographyImageDto>> GetImageByIdAsync(Guid id)
    {
        try
        {
            var image = await _context.MammographyImages
                .Include(m => m.DiagnosisResult)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (image == null)
            {
                return new ApiResponse<MammographyImageDto>
                {
                    Success = false,
                    Message = "Image non trouvée"
                };
            }

            return new ApiResponse<MammographyImageDto>
            {
                Success = true,
                Data = MapToDto(image)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image {ImageId}", id);
            return new ApiResponse<MammographyImageDto>
            {
                Success = false,
                Message = "Erreur lors de la récupération"
            };
        }
    }

    public async Task<ApiResponse<List<MammographyImageDto>>> GetImagesByPatientAsync(Guid patientId)
    {
        try
        {
            var images = await _context.MammographyImages
                .Include(m => m.DiagnosisResult)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return new ApiResponse<List<MammographyImageDto>>
            {
                Success = true,
                Data = images.Select(MapToDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting images for patient {PatientId}", patientId);
            return new ApiResponse<List<MammographyImageDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération"
            };
        }
    }

    public async Task<ApiResponse<DiagnosisResultDto>> ValidateDiagnosisAsync(ValidateDiagnosisRequest request, Guid doctorId)
    {
        try
        {
            var diagnosis = await _context.DiagnosisResults.FindAsync(request.DiagnosisId);

            if (diagnosis == null)
            {
                return new ApiResponse<DiagnosisResultDto>
                {
                    Success = false,
                    Message = "Diagnostic non trouvé"
                };
            }

            diagnosis.DoctorValidated = true;
            diagnosis.ValidatedByDoctorId = doctorId;
            diagnosis.FinalDiagnosis = request.FinalDiagnosis;
            diagnosis.DoctorNotes = request.DoctorNotes;
            diagnosis.ValidatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Diagnosis validated: {DiagnosisId} by doctor {DoctorId}", 
                request.DiagnosisId, doctorId);

            return new ApiResponse<DiagnosisResultDto>
            {
                Success = true,
                Message = "Diagnostic validé avec succès",
                Data = MapToDiagnosisDto(diagnosis)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating diagnosis {DiagnosisId}", request.DiagnosisId);
            return new ApiResponse<DiagnosisResultDto>
            {
                Success = false,
                Message = "Erreur lors de la validation"
            };
        }
    }

    public async Task<ApiResponse<AIServiceStatusDto>> GetAIServiceStatusAsync()
    {
        var health = await _aiService.CheckHealthAsync();
        
        if (health == null)
        {
            return new ApiResponse<AIServiceStatusDto>
            {
                Success = true,
                Data = new AIServiceStatusDto
                {
                    Available = false,
                    Status = "UNAVAILABLE"
                }
            };
        }

        return new ApiResponse<AIServiceStatusDto>
        {
            Success = true,
            Data = new AIServiceStatusDto
            {
                Available = health.ModelsAvailable,
                Status = health.Status,
                ImageModelLoaded = health.ImageLoaded,
                TabularModelLoaded = health.TabularLoaded,
                ImageModelError = health.ImageModelError,
                TabularModelError = health.TabularModelError
            }
        };
    }

    public async Task<ApiResponse<List<DiagnosisResultDto>>> GetPendingValidationsAsync()
    {
        try
        {
            var pending = await _context.DiagnosisResults
                .Include(d => d.Image)
                .Where(d => !d.DoctorValidated)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return new ApiResponse<List<DiagnosisResultDto>>
            {
                Success = true,
                Data = pending.Select(MapToDiagnosisDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending validations");
            return new ApiResponse<List<DiagnosisResultDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteImageAsync(Guid id)
    {
        try
        {
            var image = await _context.MammographyImages.FindAsync(id);
            if (image == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Image non trouvée",
                    Data = false
                };
            }

            // Delete file
            if (!string.IsNullOrEmpty(image.FilePath) && File.Exists(image.FilePath))
            {
                File.Delete(image.FilePath);
            }

            _context.MammographyImages.Remove(image);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Image supprimée",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression",
                Data = false
            };
        }
    }

    private static MammographyImageDto MapToDto(MammographyImage image)
    {
        return new MammographyImageDto
        {
            Id = image.Id,
            PatientId = image.PatientId,
            UploadedByUserId = image.UploadedByUserId,
            FileName = image.FileName,
            OriginalFileName = image.OriginalFileName,
            FileSizeBytes = image.FileSizeBytes,
            ContentType = image.ContentType,
            ImageType = image.ImageType,
            ViewPosition = image.ViewPosition,
            Status = image.Status,
            Notes = image.Notes,
            UploadedAt = image.UploadedAt,
            AnalyzedAt = image.AnalyzedAt,
            DiagnosisResult = image.DiagnosisResult != null ? MapToDiagnosisDto(image.DiagnosisResult) : null
        };
    }

    private static DiagnosisResultDto MapToDiagnosisDto(DiagnosisResult diagnosis)
    {
        return new DiagnosisResultDto
        {
            Id = diagnosis.Id,
            ImageId = diagnosis.ImageId,
            PatientId = diagnosis.PatientId,
            Label = diagnosis.Label,
            Probability = diagnosis.Probability,
            PImage = diagnosis.PImage,
            PTabular = diagnosis.PTabular,
            ModelVersion = diagnosis.ModelVersion,
            DegradedMode = diagnosis.DegradedMode,
            HeatmapPath = diagnosis.HeatmapPath,
            DoctorValidated = diagnosis.DoctorValidated,
            ValidatedByDoctorId = diagnosis.ValidatedByDoctorId,
            DoctorNotes = diagnosis.DoctorNotes,
            FinalDiagnosis = diagnosis.FinalDiagnosis,
            CreatedAt = diagnosis.CreatedAt,
            ValidatedAt = diagnosis.ValidatedAt
        };
    }
}

// Events for MassTransit
public record ImageUploadedEvent
{
    public Guid ImageId { get; init; }
    public Guid PatientId { get; init; }
    public DateTime UploadedAt { get; init; }
}

public record DiagnosisCompletedEvent
{
    public Guid DiagnosisId { get; init; }
    public Guid ImageId { get; init; }
    public Guid PatientId { get; init; }
    public string Label { get; init; } = string.Empty;
    public double Probability { get; init; }
    public DateTime CompletedAt { get; init; }
}
