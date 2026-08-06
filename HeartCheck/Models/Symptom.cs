using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Symptom
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("measurementId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId MeasurementId { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("confidence")]
        public double Confidence { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
