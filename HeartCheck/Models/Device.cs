using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Device
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("deviceIdentifier")]
        public string DeviceIdentifier { get; set; } = null!;

        [BsonElement("deviceModel")]
        public string DeviceModel { get; set; } = null!;

        [BsonElement("firmwareVersion")]
        public string FirmwareVersion { get; set; } = null!;

        [BsonElement("status")]
        public string Status { get; set; } = "active";

        [BsonElement("lastSync")]
        public DateTime LastSync { get; set; }

        [BsonElement("batteryLevel")]
        public int BatteryLevel { get; set; }

        [BsonElement("pairedAt")]
        public DateTime PairedAt { get; set; }

        [BsonElement("unpairedAt")]
        public DateTime? UnpairedAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
