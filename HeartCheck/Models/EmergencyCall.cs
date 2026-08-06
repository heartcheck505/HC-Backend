using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class EmergencyCall
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("alertId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AlertId { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("emergencyContactId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId EmergencyContactId { get; set; }

        [BsonElement("contactName")]
        public string ContactName { get; set; } = null!;

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = null!;

        [BsonElement("status")]
        public string Status { get; set; } = "pending";

        [BsonElement("duration")]
        public int? Duration { get; set; }

        [BsonElement("result")]
        public string? Result { get; set; }

        [BsonElement("initiatedAt")]
        public DateTime? InitiatedAt { get; set; }

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
