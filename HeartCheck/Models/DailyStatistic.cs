using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class DailyStatistic
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("averageBpm")]
        public double AverageBpm { get; set; }

        [BsonElement("minBpm")]
        public int MinBpm { get; set; }

        [BsonElement("maxBpm")]
        public int MaxBpm { get; set; }

        [BsonElement("totalMeasurements")]
        public int TotalMeasurements { get; set; }

        [BsonElement("normalMeasurements")]
        public int NormalMeasurements { get; set; }

        [BsonElement("abnormalMeasurements")]
        public int AbnormalMeasurements { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}