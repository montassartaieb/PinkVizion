using MongoDB.Driver;
using PinkVision.Notification.API.DTOs;
using PinkVision.Notification.API.Entities;

namespace PinkVision.Notification.API.Services;

public interface INotificationService
{
    Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationRequest request);
    Task<ApiResponse<List<NotificationDto>>> GetByUserAsync(string userId, bool unreadOnly = false);
    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
    Task<ApiResponse<bool>> MarkAsReadAsync(string id);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId);
    Task<ApiResponse<bool>> DeleteAsync(string id);
}

public class NotificationService : INotificationService
{
    private readonly IMongoCollection<NotificationEntity> _notifications;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDB"));
        var database = client.GetDatabase("pinkvision_notifications");
        _notifications = database.GetCollection<NotificationEntity>("notifications");
        _logger = logger;
    }

    public async Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationRequest request)
    {
        var notification = new NotificationEntity
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Data = request.Data
        };

        await _notifications.InsertOneAsync(notification);
        _logger.LogInformation("Notification created for user {UserId}: {Title}", request.UserId, request.Title);

        return new ApiResponse<NotificationDto> { Success = true, Data = MapToDto(notification) };
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetByUserAsync(string userId, bool unreadOnly = false)
    {
        var filter = Builders<NotificationEntity>.Filter.Eq(n => n.UserId, userId);
        if (unreadOnly)
            filter &= Builders<NotificationEntity>.Filter.Eq(n => n.IsRead, false);

        var notifications = await _notifications
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Limit(50)
            .ToListAsync();

        return new ApiResponse<List<NotificationDto>>
        {
            Success = true,
            Data = notifications.Select(MapToDto).ToList()
        };
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
    {
        var count = await _notifications.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
        return new ApiResponse<int> { Success = true, Data = (int)count };
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(string id)
    {
        var update = Builders<NotificationEntity>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow);

        var result = await _notifications.UpdateOneAsync(n => n.Id == id, update);
        return new ApiResponse<bool> { Success = result.ModifiedCount > 0, Data = result.ModifiedCount > 0 };
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId)
    {
        var update = Builders<NotificationEntity>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow);

        var result = await _notifications.UpdateManyAsync(n => n.UserId == userId && !n.IsRead, update);
        return new ApiResponse<bool> { Success = true, Data = result.ModifiedCount > 0 };
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string id)
    {
        var result = await _notifications.DeleteOneAsync(n => n.Id == id);
        return new ApiResponse<bool> { Success = result.DeletedCount > 0, Data = result.DeletedCount > 0 };
    }

    private static NotificationDto MapToDto(NotificationEntity n) => new()
    {
        Id = n.Id ?? string.Empty,
        UserId = n.UserId,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        Data = n.Data,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}
