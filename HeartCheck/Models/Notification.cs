using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Notification
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("message")]
        public string Message { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("readAt")]
        public DateTime? ReadAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}