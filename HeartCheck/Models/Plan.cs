using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Plan
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("maxDevices")]
        public int MaxDevices { get; set; }

        [BsonElement("measurementIntervalMinutes")]
        public int MeasurementIntervalMinutes { get; set; }

        [BsonElement("includesEmergencyCalls")]
        public bool IncludesEmergencyCalls { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
