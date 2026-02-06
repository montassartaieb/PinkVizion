using System.Net.Http.Headers;
using System.Text.Json;
using PinkVision.Imaging.API.DTOs;

namespace PinkVision.Imaging.API.Services;

public interface IAIService
{
    Task<AIHealthResponse?> CheckHealthAsync();
    Task<AIPredictResponse?> PredictAsync(Stream imageStream, string fileName, Dictionary<string, object> features);
}

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _aiServiceUrl;
    private readonly int _timeoutSeconds;

    public AIService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AIService");
        _logger = logger;
        _aiServiceUrl = configuration["AI_SERVICE_URL"] ?? configuration["AIService:Url"] ?? "http://localhost:8001";
        _timeoutSeconds = int.Parse(configuration["AIService:TimeoutSeconds"] ?? "60");
        _httpClient.BaseAddress = new Uri(_aiServiceUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    }

    public async Task<AIHealthResponse?> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AIHealthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            _logger.LogWarning("AI service health check failed: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking AI service health");
            return null;
        }
    }

    public async Task<AIPredictResponse?> PredictAsync(Stream imageStream, string fileName, Dictionary<string, object> features)
    {
        try
        {
            using var formContent = new MultipartFormDataContent();

            // Add image file
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
            formContent.Add(streamContent, "file", fileName);

            // Add features JSON
            var featuresJson = JsonSerializer.Serialize(features);
            formContent.Add(new StringContent(featuresJson), "features_json");

            _logger.LogInformation("Sending prediction request to AI service for file: {FileName}", fileName);

            var response = await _httpClient.PostAsync("/v1/predict", formContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("AI prediction successful: {Response}", content);
                
                return JsonSerializer.Deserialize<AIPredictResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("AI prediction failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return null;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("AI prediction request timed out after {Timeout}s", _timeoutSeconds);
            throw new TimeoutException($"AI service did not respond within {_timeoutSeconds} seconds");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI prediction service");
            throw;
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".dcm" => "application/dicom",
            _ => "application/octet-stream"
        };
    }
}

// DTOs for AI Service responses
public class AIHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public bool ImageLoaded { get; set; }
    public bool TabularLoaded { get; set; }
    public string? ImageModelError { get; set; }
    public string? TabularModelError { get; set; }
    public bool ModelsAvailable { get; set; }
}

public class AIPredictResponse
{
    public string Label { get; set; } = string.Empty;
    public double Probability { get; set; }
    public double PImage { get; set; }
    public double PTabular { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public bool DegradedMode { get; set; }
}
