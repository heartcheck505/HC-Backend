using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    public class MeasurementMetadata
    {
        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PatientId { get; set; }

        [BsonElement("deviceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId DeviceId { get; set; }
    }
}
