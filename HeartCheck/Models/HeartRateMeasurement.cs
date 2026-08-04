using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class HeartRateMeasurement
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("metadata")]
        public MeasurementMetadata Metadata { get; set; } = null!;

        [BsonElement("bpm")]
        public int Bpm { get; set; }

        [BsonElement("quality")]
        public string Quality { get; set; } = null!;

        [BsonElement("context")]
        public string Context { get; set; } = null!;

        [BsonElement("isNormal")]
        public bool IsNormal { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }
    }
}
