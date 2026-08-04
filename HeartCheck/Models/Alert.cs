using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Alert
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("measurementId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId MeasurementId { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("deviceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId DeviceId { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("severity")]
        public string Severity { get; set; } = null!;

        [BsonElement("bpmValue")]
        public int BpmValue { get; set; }

        [BsonElement("threshold")]
        public int Threshold { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "active";

        [BsonElement("acknowledgedAt")]
        public DateTime? AcknowledgedAt { get; set; }

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
