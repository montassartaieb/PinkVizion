using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PinkVision.Notification.API.Entities;

public class NotificationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // APPOINTMENT, DIAGNOSIS, SYSTEM

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("data")]
    public Dictionary<string, object>? Data { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
