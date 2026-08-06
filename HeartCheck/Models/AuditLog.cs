using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class AuditLog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("entity")]
        public string Entity { get; set; } = string.Empty;

        [BsonElement("entityId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId EntityId { get; set; }

        [BsonElement("previousValues")]
        public BsonDocument? PreviousValues { get; set; }

        [BsonElement("newValues")]
        public BsonDocument? NewValues { get; set; }

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [BsonElement("userAgent")]
        public string? UserAgent { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
