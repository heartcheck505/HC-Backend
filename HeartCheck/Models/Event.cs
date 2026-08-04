using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Event
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("alertId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AlertId { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("userResponse")]
        public string? UserResponse { get; set; }

        [BsonElement("respondedAt")]
        public DateTime RespondedAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
