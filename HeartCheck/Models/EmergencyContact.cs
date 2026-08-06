using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    public class EmergencyContact
    {
        [BsonElement("id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("relationship")]
        public string Relationship { get; set; } = null!;

        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("isPrimary")]
        public bool IsPrimary { get; set; }
    }
}
